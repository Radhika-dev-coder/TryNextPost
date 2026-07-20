using FluentValidation;
using TryNextPost.Application.DTO.Shipment;

namespace TryNextPost.Application.Validators.Shipment
{
    public sealed class ShipmentTrackingWebhookRequestValidator : AbstractValidator<ShipmentTrackingWebhookRequest>
    {
        public ShipmentTrackingWebhookRequestValidator()
        {
            RuleFor(x => x.AwbNumber)
                .NotEmpty()
                .MaximumLength(100)
                .WithMessage("AwbNumber is required.");

            RuleFor(x => x.Status)
                .NotEmpty()
                .When(x => !x.StatusCode.HasValue)
                .WithMessage("Status or StatusCode is required.");

            RuleFor(x => x.Location)
                .MaximumLength(200)
                .When(x => !string.IsNullOrWhiteSpace(x.Location));

            RuleFor(x => x.Description)
                .MaximumLength(1000)
                .When(x => !string.IsNullOrWhiteSpace(x.Description));

            RuleFor(x => x.CourierCode)
                .MaximumLength(50)
                .When(x => !string.IsNullOrWhiteSpace(x.CourierCode));
        }
    }
}
