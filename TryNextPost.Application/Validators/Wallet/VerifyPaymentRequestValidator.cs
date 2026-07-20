using FluentValidation;
using TryNextPost.Application.DTO.Wallet;

namespace TryNextPost.Application.Validators.Wallet
{
    public sealed class VerifyPaymentRequestValidator : AbstractValidator<VerifyPaymentRequest>
    {
        public VerifyPaymentRequestValidator()
        {
            RuleFor(x => x.RazorpayOrderId)
                .NotEmpty()
                .WithMessage("RazorpayOrderId is required.")
                .MaximumLength(100);

            RuleFor(x => x.RazorpayPaymentId)
                .NotEmpty()
                .WithMessage("RazorpayPaymentId is required.")
                .MaximumLength(100);

            RuleFor(x => x.RazorpaySignature)
                .NotEmpty()
                .WithMessage("RazorpaySignature is required.")
                .MaximumLength(256);
        }
    }
}
