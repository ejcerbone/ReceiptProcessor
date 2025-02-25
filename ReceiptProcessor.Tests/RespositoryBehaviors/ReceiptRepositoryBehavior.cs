using Moq;
using ReceiptProcessor.Models;
using ReceiptProcessor.Repositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ReceiptProcessor.Tests.RespositoryBehaviors
{
    [TestFixture]
    public class ReceiptRepositoryBehavior
    {
        private ReceiptRepository _repository;

        [SetUp]
        public void Setup()
        {
            _repository = new ReceiptRepository();
        }

        [Test]
        public void Should_Store_Receipt_And_Return_ReceiptId()
        {
            var receipt = new Receipt
            {
                retailer = "Test Retailer",
                purchaseDate = "2025-02-24",
                purchaseTime = "14:30",
                total = 99.99m,
                points = 10
            };

            var receiptId = _repository.SaveReceipt(receipt);

            Assert.That(receiptId, Is.Not.EqualTo(Guid.Empty));
            var storedReceipt = _repository.GetReceipt(receiptId);
            Assert.That(receipt, Is.EqualTo(storedReceipt));
        }

        [Test]
        public void Should_Return_Correct_Receipt_By_Id()
        {
            var receipt = new Receipt
            {
                retailer = "Another Retailer",
                purchaseDate = "2025-02-25",
                purchaseTime = "10:00",
                total = 49.99m,
                points = 20
            };

            var receiptId = _repository.SaveReceipt(receipt);

            var retrievedReceipt = _repository.GetReceipt(receiptId);

            Assert.That(retrievedReceipt, Is.Not.Null);
            Assert.That(retrievedReceipt.retailer, Is.EqualTo(receipt.retailer));
        }

        [Test]
        public void Should_Update_Points_For_Existing_Receipt()
        {
            var receipt = new Receipt
            {
                retailer = "Score Retailer",
                purchaseDate = "2025-02-26",
                purchaseTime = "08:30",
                total = 29.99m,
                points = 0
            };

            var receiptId = _repository.SaveReceipt(receipt);

            _repository.UpdateScore(receiptId, 50);
            var updatedReceipt = _repository.GetReceipt(receiptId);

            Assert.That(updatedReceipt.points, Is.EqualTo(50));
        }

        [Test]
        public void Should_Throw_Exception_When_Receipt_Not_Found()
        {
            var nonExistentReceiptId = Guid.NewGuid();

            Assert.Throws<KeyNotFoundException>(() => _repository.GetReceipt(nonExistentReceiptId));
        }
    }

}
