using Microsoft.Extensions.Configuration;
using ReceiptProcessor.Models;
using System.Collections.Concurrent;
using System.Threading.Channels;
using System.Threading.Tasks;

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
            var watch = System.Diagnostics.Stopwatch.StartNew();

            int score = 0;

            receiptStatus[task.ReceiptId] = ReceiptProcessingStatus.Processing;

            var receipt = await _receiptServices.GetReceipt(task.ReceiptId);

            //Optimization to run all the calculations in parallel using Task(async)
            var tasks = new List<Task<int>>();
            tasks.Add(Task.Run(() => ScoreAlphanumeric(receipt)));
            tasks.Add(Task.Run(() => ScoreItemPairs(receipt)));
            tasks.Add(Task.Run(() => TotalIsEven(receipt)));
            tasks.Add(Task.Run(() => MultipleOf25(receipt)));
            tasks.Add(Task.Run(() => ScoreDate(receipt)));
            tasks.Add(Task.Run(() => ScoreBetween14n16(receipt)));
            tasks.Add(Task.Run(() => ScoreItems(receipt)));

            int[] results = await Task.WhenAll(tasks);
        
            score = results.Sum();

            await _receiptServices.UpdateScore(receipt.receiptId, score);

            //simulate scoring delay, used to test the status endpoint
            //await Task.Delay(5000);

            watch.Stop();

            logger.LogInformation($"Receipt id:{task.ReceiptId} score timing, {watch.ElapsedMilliseconds}ms");
            

            return score;
        }

        private static int ScoreItems(Receipt receipt)
        {
            int score = 0;

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

            return score;
        }

        private static int ScoreItemPairs(Receipt receipt)
        {

            int score = 0;

            if (receipt.items != null && receipt.items.Count > 1)
            {
                score += (int)(Decimal.Floor(receipt.items.Count / 2) * 5);
            }

            return score;
        }

        private static int ScoreBetween14n16(Receipt receipt)
        {

            int score = 0;

            var time = DateTime.Parse(receipt.purchaseTime).TimeOfDay;
            if (receipt.purchaseTime != null && time > new TimeSpan(14, 0, 0) && time < new TimeSpan(16, 0, 0))
            {
                score += 10;
            }

            return score;
        }

        private static int ScoreAlphanumeric(Receipt receipt)
        {

            int score = 0;

            score += receipt.retailer.Count(x => char.IsLetterOrDigit(x));
            return score;
        }

        private static int TotalIsEven(Receipt receipt)
        {

            int score = 0;

            if (receipt.total % 1 == 0)
            {
                score += 50;
            }

            return score;
        }

        private static int MultipleOf25(Receipt receipt)
        {

            int score = 0;

            if (receipt.total % 0.25m == 0)
            {
                score += 25;
            }

            return score;
        }

        private static int ScoreDate(Receipt receipt)
        {

            int score = 0;

            if (receipt.purchaseDate != null && DateTime.Parse(receipt.purchaseDate).Day % 2 != 0)
            {
                score += 6;
            }

            return score;
        }
    }
}
