using Microsoft.Extensions.Logging;
using Moq;
using ReceiptProcessor.Models;
using ReceiptProcessor.Services;
using System.Collections.Concurrent;
using System.Threading.Channels;

namespace ReceiptProcessor.Tests.ScoringBehaviors
{
    public class ReceiptScoringServiceBehavior
    {
        private Mock<IReceiptServices> _receiptServicesMock;
        private Mock<ILogger<ReceiptScoringService>> _loggerMock;
        private Channel<ReceiptProcessorTask> _channel;
        private ConcurrentDictionary<Guid, ReceiptProcessingStatus> _receiptStatus;
        private ReceiptScoringService _service;

        [SetUp]
        public void Setup()
        {
            _receiptServicesMock = new Mock<IReceiptServices>();
            _loggerMock = new Mock<ILogger<ReceiptScoringService>>();
            _channel = Channel.CreateUnbounded<ReceiptProcessorTask>();
            _receiptStatus = new ConcurrentDictionary<Guid, ReceiptProcessingStatus>();
            _service = new ReceiptScoringService(_receiptServicesMock.Object, _channel, _receiptStatus, _loggerMock.Object);
        }

        [Test]
        public async Task Should_Process_Receipt_And_Update_Status()
        {
            var receiptId = Guid.NewGuid();
            var receipt = new Receipt
            {
                receiptId = receiptId,
                retailer = "Retailer123",
                purchaseDate = "2025-02-24",
                purchaseTime = "15:00",
                items = new List<ReceiptItem>
            {
                new ReceiptItem { shortDescription = "Item1", price = 10m },
                new ReceiptItem { shortDescription = "Item2", price = 5m }
            },
                total = 100m
            };

            var task = new ReceiptProcessorTask(receiptId);

            _receiptServicesMock.Setup(s => s.GetReceipt(receiptId)).ReturnsAsync(receipt);

            await _channel.Writer.WriteAsync(task);


            var cancellationToken = new System.Threading.CancellationTokenSource(1000).Token;
            await _service.StartAsync(cancellationToken);


            _receiptServicesMock.Verify(s => s.UpdateScore(receiptId, It.IsAny<int>()), Times.Once);
            Assert.That(_receiptStatus[receiptId], Is.EqualTo(ReceiptProcessingStatus.Processed));
        }

        [Test]
        public async Task Should_Set_Status_To_Failed_On_Exception()
        {
            var receiptId = Guid.NewGuid();
            var task = new ReceiptProcessorTask(receiptId);

            _receiptServicesMock.Setup(s => s.GetReceipt(receiptId)).ThrowsAsync(new Exception("Score Exception"));
            await _channel.Writer.WriteAsync(task);

            var cancellationToken = new System.Threading.CancellationTokenSource(1000).Token;
            await _service.StartAsync(cancellationToken);

            Assert.That(_receiptStatus[receiptId], Is.EqualTo(ReceiptProcessingStatus.Failed));
        }

        [TearDown]
        public void TearDown() {
            _service.Dispose();
        }
        
    }

}
