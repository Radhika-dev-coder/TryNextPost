using FluentValidation;
using TryNextPost.Application.DTO.Auth;
using TryNextPost.Domain.Common;

namespace TryNextPost.Application.DTO
{
    public sealed class ResetPasswordRequestValidator : AbstractValidator<ResetPasswordRequest>
    {
        public ResetPasswordRequestValidator()
        {
            RuleFor(x => x.ResetToken)
                .NotEmpty();

            RuleFor(x => x.NewPassword)
                .NotEmpty()
                .MinimumLength(6);

            RuleFor(x => x.ConfirmPassword)
                .Equal(x => x.NewPassword)
                .WithMessage(SystemMessage.PasswordMismatch);
        }
    }
}
