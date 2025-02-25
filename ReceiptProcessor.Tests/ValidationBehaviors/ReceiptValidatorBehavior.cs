using ReceiptProcessor.Models;
using ReceiptProcessor.Validation;
using System;
using FluentValidation.TestHelper;

namespace ReceiptProcessor.Tests.ValidationBehaviors
{
    public class ReceiptValidatorBehavior
    {
        private ReceiptValidator _validator;

        [SetUp]
        public void Setup()
        {
            _validator = new ReceiptValidator();
        }

        [Test]
        public void Should_Have_Error_When_Retailer_Is_Empty()
        {
            var receipt = new Receipt { 
                retailer = "",
                purchaseDate = "2025-1d3-01",
                purchaseTime= "25:00",
                total = 0m,
                points = 76

            };
            var result = _validator.TestValidate(receipt);
            result.ShouldHaveValidationErrorFor(r => r.retailer)
                  .WithErrorMessage("Must have a retailer name.");
        }

        [Test]
        public void Should_Have_Error_When_Retailer_Lacks_Alphanumeric_Characters()
        {
            var receipt = new Receipt { 
                retailer = "!!!",
                purchaseDate = "2025-1d3-01",
                purchaseTime = "25:00",
                total = 0m,
                points = 76
            };
            var result = _validator.TestValidate(receipt);
            result.ShouldHaveValidationErrorFor(r => r.retailer)
                  .WithErrorMessage("Must contain at least one alphanumeric character.");
        }

        [Test]
        public void Should_Have_Error_When_PurchaseTime_Is_Invalid()
        {
            var receipt = new Receipt
            {
                retailer = "!!!",
                purchaseDate = "2025-1d3-01",
                purchaseTime = "25:00",
                total = 0m,
                points = 76
            };
            var result = _validator.TestValidate(receipt);
            result.ShouldHaveValidationErrorFor(r => r.purchaseTime)
                  .WithErrorMessage("Invalid purchase time format. Should be in the 24-hour time format. ");
        }

        [Test]
        public void Should_Have_Error_When_PurchaseDate_Is_Invalid()
        {
            var receipt = new Receipt
            {
                retailer = "!!!",
                purchaseDate = "2025-1d3-01",
                purchaseTime = "25:00",
                total = 0m,
                points = 76
            };
            var result = _validator.TestValidate(receipt);
            result.ShouldHaveValidationErrorFor(r => r.purchaseDate)
                  .WithErrorMessage("Invalid purchase date format. Expecting yyyy-mm-dd.");
        }

        [Test]
        public void Should_Have_Error_When_Total_Is_Empty()
        {
            var receipt = new Receipt
            {
                retailer = "!!!",
                purchaseDate = "2025-1d3-01",
                purchaseTime = "25:00",
                total = 0m,
                points = 76
            };
            var result = _validator.TestValidate(receipt);
            result.ShouldHaveValidationErrorFor(r => r.total)
                  .WithErrorMessage("Must have a total.");
        }

        [Test]
        public void Should_Not_Have_Error_When_All_Fields_Are_Valid()
        {
            var receipt = new Receipt
            {
                retailer = "Valid Retailer",
                purchaseDate = "2025-02-24",
                purchaseTime = "14:30",
                total = 99.99m
            };

            var result = _validator.TestValidate(receipt);
            result.ShouldNotHaveAnyValidationErrors();
        }
    }

}
