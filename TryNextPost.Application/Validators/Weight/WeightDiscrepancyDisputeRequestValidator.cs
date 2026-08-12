using FluentValidation;
using TryNextPost.Application.DTO.Weight;

namespace TryNextPost.Application.Validators.Weight
{
    public sealed class WeightDiscrepancyDisputeRequestValidator : AbstractValidator<WeightDiscrepancyDisputeRequest>
    {
        public WeightDiscrepancyDisputeRequestValidator()
        {
            RuleFor(x => x.Remarks).MaximumLength(500);
        }
    }
}
