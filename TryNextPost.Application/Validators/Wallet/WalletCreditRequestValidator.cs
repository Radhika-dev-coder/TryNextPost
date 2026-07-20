using FluentValidation;
using TryNextPost.Application.DTO.Wallet;

namespace TryNextPost.Application.Validators.Wallet
{
    public sealed class WalletCreditRequestValidator : AbstractValidator<WalletCreditRequest>
    {
        public WalletCreditRequestValidator()
        {
            RuleFor(x => x.UserId)
                .NotEmpty()
                .WithMessage("UserId is required.")
                .MaximumLength(100);

            RuleFor(x => x.Amount)
                .GreaterThan(0)
                .WithMessage("Amount must be greater than zero.");

            RuleFor(x => x.Description)
                .MaximumLength(500)
                .When(x => !string.IsNullOrWhiteSpace(x.Description));

            RuleFor(x => x.ReferenceId)
                .MaximumLength(100)
                .When(x => !string.IsNullOrWhiteSpace(x.ReferenceId));
        }
    }
}
