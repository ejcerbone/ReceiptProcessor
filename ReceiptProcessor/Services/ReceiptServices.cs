using ReceiptProcessor.Models;
using ReceiptProcessor.Repositories;
using System.Collections.Concurrent;
using System.Threading.Channels;

namespace ReceiptProcessor.Services
{
    public class ReceiptServices(IReceiptRepository repo, Channel<ReceiptProcessorTask> channel,
            ConcurrentDictionary<Guid, ReceiptProcessingStatus> receiptStatus) : IReceiptServices
    {
        private readonly IReceiptRepository _repo = repo;
        private readonly ConcurrentDictionary<Guid, ReceiptProcessingStatus> _status = receiptStatus;
        private readonly Channel<ReceiptProcessorTask> _channel = channel;

        public async Task<Receipt> GetReceipt(Guid receiptId)
        {
            var receipt = _repo.GetReceipt(receiptId);

            return await Task.FromResult(receipt);
        }

        public async Task<ReceiptProcessingStatus> GetReceiptStatus(Guid receiptId)
        {
            return await Task.FromResult(_status[receiptId]);
        }

        public async Task<Guid> SaveReceipt(Receipt receipt)
        {
            _repo.SaveReceipt(receipt);

            var task = new ReceiptProcessorTask(receipt.receiptId);
            await _channel.Writer.WriteAsync(task);

            _status[receipt.receiptId] = ReceiptProcessingStatus.Pending;

            return receipt.receiptId;
        }

        public async Task<bool> UpdateScore(Guid receiptId, int score)
        {
            _repo.UpdateScore(receiptId, score);

            return await Task.FromResult(true);
        }


    }
}
