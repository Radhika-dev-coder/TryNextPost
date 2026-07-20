using FluentValidation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TryNextPost.Application.DTO.Auth;

namespace TryNextPost.Application.DTO
{
    public sealed class SendPhoneOtpRequestValidator : AbstractValidator<SendPhoneOtpRequest>
    {
        public SendPhoneOtpRequestValidator()
        {
            RuleFor(x => x.Mobile)
                .NotEmpty()
                .Matches(@"^[6-9]\d{9}$")
                .WithMessage("Enter a valid 10-digit Indian mobile number");
        }
    }
}
