using FluentValidation;
using ReceiptProcessor.Models;

namespace ReceiptProcessor.Validation
{
    public class ReceiptValidator : AbstractValidator<Receipt>
    {
        public ReceiptValidator()
        {

            RuleFor(r => r.retailer).NotNull().NotEmpty().WithMessage("Must have a retailer name.");
            //I wasn't aware of any retailer that doesn't have any alphanumeric characters in it's name, quick google seemed to align to that thought. 
            RuleFor(r => r.retailer).Matches(@".*[a-zA-Z0-9].*").WithMessage("Must contain at least one alphanumeric character.");

            RuleFor(r => r.purchaseTime).NotNull().NotEmpty().WithMessage("Must provide a purchase time.");
            RuleFor(r => r.purchaseTime).Matches(@"^([01]?[0-9]|2[0-3]):[0-5][0-9]$").WithMessage("Invalid purchase time format. Should be in the 24-hour time format. ");

            RuleFor(r => r.purchaseDate).NotNull().NotEmpty().WithMessage("Must provide a purchase date.");
            RuleFor(r => r.purchaseDate).Matches(@"^\d{4}\-(0[1-9]|1[012])\-(0[1-9]|[12][0-9]|3[01])$").WithMessage("Invalid purchase date format. Expecting yyyy-mm-dd.");

            RuleFor(r => r.total).NotNull().NotEmpty().WithMessage("Must have a total.");

            //with more time this could be expanded to do a deeper validation
        }
    }
}
