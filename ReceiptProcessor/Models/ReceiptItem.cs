namespace ReceiptProcessor.Models
{
    public class ReceiptItem
    {
        public required string shortDescription { get; set; }
        public required decimal price { get; set; }
    }
}
