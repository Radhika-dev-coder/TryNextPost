using FluentValidation;
using TryNextPost.Application.DTO.Wallet;

namespace TryNextPost.Application.Validators.Wallet
{
    public sealed class WalletRechargeRequestValidator : AbstractValidator<WalletRechargeRequest>
    {
        public WalletRechargeRequestValidator()
        {
            RuleFor(x => x.Amount)
                .GreaterThan(0)
                .WithMessage("Amount must be greater than zero.")
                .LessThanOrEqualTo(500000)
                .WithMessage("Amount cannot exceed 500000 INR per recharge.");
        }
    }
}
