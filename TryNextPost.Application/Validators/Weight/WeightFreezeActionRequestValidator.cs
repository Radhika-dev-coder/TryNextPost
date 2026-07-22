using FluentValidation;
using TryNextPost.Application.DTO.Weight;

namespace TryNextPost.Application.Validators.Weight
{
    public sealed class WeightFreezeActionRequestValidator : AbstractValidator<WeightFreezeActionRequest>
    {
        public WeightFreezeActionRequestValidator()
        {
            RuleFor(x => x.Action)
                .NotEmpty().WithMessage("Action is required.")
                .Must(a =>
                {
                    var key = (a ?? string.Empty).Trim().ToLowerInvariant();
                    return key is "accept" or "accepted" or "reject" or "rejected";
                })
                .WithMessage("Action must be Accept or Reject.");

            RuleFor(x => x.Remarks).MaximumLength(500);
        }
    }
}
