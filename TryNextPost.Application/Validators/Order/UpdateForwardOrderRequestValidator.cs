using FluentValidation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TryNextPost.Application.DTO.Order;

namespace TryNextPost.Application.Validators.Order
{
    public class UpdateForwardOrderRequestValidator : AbstractValidator<UpdateForwardOrderRequest>
    {
        public UpdateForwardOrderRequestValidator()
        {
            RuleFor(x => x.PaymentMode)
                .InclusiveBetween(1, 2)
                .WithMessage("Invalid Payment Mode");
            RuleFor(x => x.CustomerName)
                .NotEmpty().WithMessage("Customer Name is required");
            RuleFor(x => x.CustomerMobile)
                .NotEmpty().WithMessage("Customer Mobile is required")
                .Matches(@"^\d{10}$").WithMessage("Invalid mobile number");
            RuleFor(x => x.ShippingAddressLine1)
                .NotEmpty().WithMessage("Shipping Address is required");
            RuleFor(x => x.ShippingPincode)
                .Matches(@"^\d{6}$").WithMessage("Pincode must be 6 digits");
            RuleFor(x => x.ShippingCity).NotEmpty();
            RuleFor(x => x.ShippingState).NotEmpty();
            RuleFor(x => x.ShippingCountry).NotEmpty();
            RuleFor(x => x.Items)
                .NotEmpty().WithMessage("At least one item is required");
            RuleForEach(x => x.Items).ChildRules(item =>
            {
                item.RuleFor(i => i.ProductName).NotEmpty();
                item.RuleFor(i => i.Qty).GreaterThan(0);
                item.RuleFor(i => i.Price).GreaterThanOrEqualTo(0);
            });
            RuleFor(x => x.WeightGrams)
                .GreaterThan(0)
                .WithMessage("Weight must be greater than 0");
            RuleFor(x => x.LengthCm).GreaterThan(0);
            RuleFor(x => x.BreadthCm).GreaterThan(0);
            RuleFor(x => x.HeightCm).GreaterThan(0);
            RuleFor(x => x.ShippingCharges).GreaterThanOrEqualTo(0);
            RuleFor(x => x.CodCharges).GreaterThanOrEqualTo(0);
            RuleFor(x => x.TaxAmount).GreaterThanOrEqualTo(0);
            RuleFor(x => x.Discount).GreaterThanOrEqualTo(0);
            When(x => x.IsCollectableAmountDifferent, () =>
            {
                RuleFor(x => x.CollectableAmount)
                    .NotNull().GreaterThanOrEqualTo(0)
                    .WithMessage("Collectable amount is required");
            });
        }
    }
}
