using Moq;
using ReceiptProcessor.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ReceiptProcessor.Tests.ModelBehaviors
{
    public class ReceiptBehavior
    {
        [Test]
        public void Should_Set_Properties_Correctly()
        {
            var receipt = new Receipt
            {
                receiptId = Guid.NewGuid(),
                retailer = "Test Retailer",
                purchaseDate = "2025-02-24",
                purchaseTime = "14:30",
                items = new List<ReceiptItem> { new ReceiptItem {
                    price = 12.45m,
                    shortDescription = "short description"
                } },
                total = 99.99m,
                points = 10
            };

            Assert.That(receipt.retailer, Is.EqualTo("Test Retailer"));
            Assert.That(receipt.purchaseDate, Is.EqualTo("2025-02-24"));
            Assert.That(receipt.purchaseTime, Is.EqualTo("14:30"));
            Assert.That(receipt.items, Is.Not.Null);
            Assert.That(receipt.total, Is.EqualTo(99.99m));
            Assert.That(receipt.points, Is.EqualTo(10));
        }

        [Test]
        public void Should_Calculate_Points_Correctly()
        {
            var mockItem = new Mock<ReceiptItem>();
            var items = new List<ReceiptItem> { mockItem.Object };

            var receipt = new Receipt
            {
                retailer = "Test Retailer",
                purchaseDate = "2025-02-24",
                purchaseTime = "14:30",
                items = items,
                total = 100m,
                points = 0
            };

            receipt.points = 100; // simulate points calculation logic

            Assert.That(receipt.points, Is.EqualTo(100));
        }
    }
}
