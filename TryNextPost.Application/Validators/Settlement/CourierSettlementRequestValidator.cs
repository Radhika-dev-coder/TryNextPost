using FluentValidation;
using TryNextPost.Application.DTO.Settlement;
using TryNextPost.Domain.Common;

namespace TryNextPost.Application.Validators.Settlement
{
    public class CreateCourierSettlementRequestValidator : AbstractValidator<CreateCourierSettlementRequest>
    {
        public CreateCourierSettlementRequestValidator()
        {
            RuleFor(x => x.CourierId)
                .GreaterThan(0)
                .WithMessage(SystemMessage.CourierNotFound);

            RuleFor(x => x.PeriodTo)
                .GreaterThanOrEqualTo(x => x.PeriodFrom)
                .WithMessage(SystemMessage.CourierSettlementInvalidPeriod);
        }
    }

    public class MarkCourierSettlementPaidRequestValidator : AbstractValidator<MarkCourierSettlementPaidRequest>
    {
        public MarkCourierSettlementPaidRequestValidator()
        {
            RuleFor(x => x.CourierSettlementId)
                .GreaterThan(0)
                .WithMessage(SystemMessage.CourierSettlementNotFound);

            RuleFor(x => x.PaymentReference)
                .NotEmpty()
                .WithMessage(SystemMessage.CourierSettlementPaymentRefRequired);
        }
    }
}
