using FluentValidation;
using TryNextPost.Application.DTO.Order;

namespace TryNextPost.Application.Validators.Order
{
    public sealed class CreateB2BOrderRequestValidator
        : CreateOrderRequestBaseValidator<CreateB2BOrderRequest>
    {
        public CreateB2BOrderRequestValidator()
        {
            RuleFor(x => x.WeightGrams)
                .GreaterThan(0)
                .WithMessage("Weight must be greater than 0");
            RuleFor(x => x.LengthCm).GreaterThan(0);
            RuleFor(x => x.BreadthCm).GreaterThan(0);
            RuleFor(x => x.HeightCm).GreaterThan(0);
            RuleFor(x => x.BillingAddressId).GreaterThan(0);
        }
    }
}
