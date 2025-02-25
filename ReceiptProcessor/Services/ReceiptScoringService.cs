using ReceiptProcessor.Models;
using System.Collections.Concurrent;
using System.Threading.Channels;

namespace ReceiptProcessor.Services
{
    public class ReceiptScoringService(IReceiptServices receiptServices, Channel<ReceiptProcessorTask> channel,
        ConcurrentDictionary<Guid, ReceiptProcessingStatus> receiptStatus, ILogger<ReceiptScoringService> logger) : BackgroundService
    {
        readonly IReceiptServices _receiptServices = receiptServices;

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            await foreach (var task in channel.Reader.ReadAllAsync(stoppingToken))
            {
                try
                {
                    await ScoreReceipt(task);
                    logger.LogInformation($"Receipt {task.ReceiptId} scored");

                    receiptStatus[task.ReceiptId] = ReceiptProcessingStatus.Processed;
                    logger.LogInformation($"Receipt {task.ReceiptId} processed");
                }
                catch (Exception ex)
                {
                    receiptStatus[task.ReceiptId] = ReceiptProcessingStatus.Failed;
                    logger.LogError(ex, $"Error processing receipt {task.ReceiptId}");

                    // you could have a retry with exponential backoff here and then dead letter if it needs intervention
                }
            }
        }

        private async Task<int> ScoreReceipt(ReceiptProcessorTask task)
        {
            int score = 0;

            receiptStatus[task.ReceiptId] = ReceiptProcessingStatus.Processing;

            var receipt = await _receiptServices.GetReceipt(task.ReceiptId);


            //alphanumeric character check.
            score += receipt.retailer.Count(x => char.IsLetterOrDigit(x));

            //every two items, not worth computing if the item count is less than 2
            if (receipt.items != null && receipt.items.Count > 1)
            {
                score += (int)(Decimal.Floor(receipt.items.Count / 2) * 5);
            }

            //total is a round
            if (receipt.total % 1 == 0)
            {
                score += 50;
            }

            //total is a multiple of 0.25, forcing to use decimal instead of double with "m" suffix
            if (receipt.total % 0.25m == 0)
            {
                score += 25;
            }

            //purchase date is odd
            if (receipt.purchaseDate != null && DateTime.Parse(receipt.purchaseDate).Day % 2 != 0)
            {
                score += 6;
            }

            //between 2:00pm(14) and before 4:00pm(16)
            var time = DateTime.Parse(receipt.purchaseTime).TimeOfDay;
            if (receipt.purchaseTime != null && time > new TimeSpan(14, 0, 0) && time < new TimeSpan(16, 0, 0))
            {
                score += 10;
            }

            //iterate the items and calculate the score
            if (receipt.items != null && receipt.items.Count > 0)
            {
                foreach (var item in receipt.items)
                {
                    if (item.shortDescription != null && item.shortDescription.Trim().Length % 3 == 0)
                    {
                        score += (int)Math.Ceiling(item.price * Convert.ToDecimal(0.2));
                    }
                }
            }

            await _receiptServices.UpdateScore(receipt.receiptId, score);

            //simulate scoring delay, used to test the status endpoint
            //await Task.Delay(5000);

            return score;
        }


    }
}
