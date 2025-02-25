namespace ReceiptProcessor.Models
{
    public class Receipt
    {
        public Guid receiptId { get; set; }
        public required string retailer { get; set; }
        public required string purchaseDate { get; set; }
        public required string purchaseTime { get; set; }
        public List<ReceiptItem>? items { get; set; }
        public required decimal total { get; set; }
        public int points { get; set; }
    }
}
