using FluentValidation;
using TryNextPost.Application.DTO.Weight;
using TryNextPost.Domain.Common;

namespace TryNextPost.Application.Validators.Weight
{
    public sealed class CreateWeightFreezeRequestValidator : AbstractValidator<CreateWeightFreezeRequest>
    {
        public CreateWeightFreezeRequestValidator()
        {
            RuleFor(x => x.ProductId)
                .NotEmpty().WithMessage(SystemMessage.WeightFreezeProductIdRequired)
                .MaximumLength(100);

            RuleFor(x => x.ProductName)
                .NotEmpty().WithMessage(SystemMessage.WeightFreezeProductNameRequired)
                .MaximumLength(250);

            RuleFor(x => x.Sku).MaximumLength(100);

            RuleFor(x => x.LengthCm).GreaterThanOrEqualTo(0);
            RuleFor(x => x.BreadthCm).GreaterThanOrEqualTo(0);
            RuleFor(x => x.HeightCm).GreaterThanOrEqualTo(0);
            RuleFor(x => x.WeightGrams).GreaterThan(0).WithMessage(SystemMessage.WeightFreezeWeightRequired);
        }
    }
}
