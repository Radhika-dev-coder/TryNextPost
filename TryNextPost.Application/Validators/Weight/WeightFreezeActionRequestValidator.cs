using FluentValidation;
using TryNextPost.Application.DTO.Weight;
using TryNextPost.Domain.Common;

namespace TryNextPost.Application.Validators.Weight
{
    public sealed class WeightFreezeActionRequestValidator : AbstractValidator<WeightFreezeActionRequest>
    {
        public WeightFreezeActionRequestValidator()
        {
            RuleFor(x => x.Action)
                .NotEmpty().WithMessage(SystemMessage.WeightFreezeActionRequired)
                .Must(a =>
                {
                    var key = (a ?? string.Empty).Trim().ToLowerInvariant();
                    return key is "accept" or "accepted" or "reject" or "rejected";
                })
                .WithMessage(SystemMessage.WeightFreezeActionInvalid);

            RuleFor(x => x.Remarks).MaximumLength(500);
        }
    }
}
