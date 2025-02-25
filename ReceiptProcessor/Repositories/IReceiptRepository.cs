using ReceiptProcessor.Models;

namespace ReceiptProcessor.Repositories
{
    public interface IReceiptRepository
    {
        Receipt GetReceipt(Guid receiptId);
        Guid SaveReceipt(Receipt receipt);
        void UpdateScore(Guid receiptId, int score);
    }
}
