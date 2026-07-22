using FluentValidation;
using TryNextPost.Application.DTO.Auth;

namespace TryNextPost.Application.DTO
{
    public sealed class ForgotPasswordRequestValidator : AbstractValidator<ForgotPasswordRequest>
    {
        public ForgotPasswordRequestValidator()
        {
            RuleFor(x => x.Email)
                .NotEmpty()
                .EmailAddress();
        }
    }
}
