using ReceiptProcessor.Models;

namespace ReceiptProcessor.Services
{
    public interface IReceiptServices
    {
        Task<Receipt> GetReceipt(Guid receiptId);
        Task<ReceiptProcessingStatus> GetReceiptStatus(Guid receiptId);
        Task<Guid> SaveReceipt(Receipt receipt);
        Task<bool> UpdateScore(Guid receiptId, int score);
    }
}
