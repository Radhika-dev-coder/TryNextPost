using FluentValidation;
using TryNextPost.Application.DTO.Shipment;

namespace TryNextPost.Application.Validators.Shipment
{
    public sealed class CancelShipmentRequestValidator : AbstractValidator<CancelShipmentRequest>
    {
        public CancelShipmentRequestValidator()
        {
            RuleFor(x => x.Reason)
                .MaximumLength(500)
                .When(x => !string.IsNullOrWhiteSpace(x.Reason));
        }
    }
}
