using FluentValidation;
using TryNextPost.Application.DTO.Shipment;

namespace TryNextPost.Application.Validators.Shipment
{
    public sealed class ConfirmShipmentRequestValidator : AbstractValidator<ConfirmShipmentRequest>
    {
        public ConfirmShipmentRequestValidator()
        {
            RuleFor(x => x.OrderId)
                .GreaterThan(0)
                .WithMessage("OrderId is required.");

            RuleFor(x => x)
                .Must(x => x.CourierId.HasValue && x.CourierId > 0 || !string.IsNullOrWhiteSpace(x.CourierCode))
                .WithMessage("CourierId or CourierCode is required.");

            RuleFor(x => x.ChargeAmount)
                .GreaterThan(0)
                .WithMessage("ChargeAmount must be greater than zero.");

            RuleFor(x => x.ServiceCode)
                .MaximumLength(100)
                .When(x => !string.IsNullOrWhiteSpace(x.ServiceCode));

            RuleFor(x => x.CourierCode)
                .MaximumLength(50)
                .When(x => !string.IsNullOrWhiteSpace(x.CourierCode));
        }
    }
}
