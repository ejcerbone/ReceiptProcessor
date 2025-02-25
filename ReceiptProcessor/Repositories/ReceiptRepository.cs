using ReceiptProcessor.Models;

namespace ReceiptProcessor.Repositories
{
    public class ReceiptRepository : IReceiptRepository
    {
        Dictionary<Guid, Receipt> _receipts = new Dictionary<Guid, Receipt>();

        public Receipt GetReceipt(Guid receiptId)
        {
            return _receipts[receiptId];
        }

        public Guid SaveReceipt(Receipt receipt)
        {
            var receiptId = Guid.NewGuid();
            receipt.receiptId = receiptId;

            _receipts[receipt.receiptId] = receipt;

            return receiptId;
        }

        public void UpdateScore(Guid receiptId, int score)
        {
            _receipts[receiptId].points = score;
        }


    }
}
