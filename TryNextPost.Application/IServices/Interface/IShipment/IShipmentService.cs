using TryNextPost.Application.DTO.Shipment;

namespace TryNextPost.Application.IServices.Interface.IShipment
{
    public interface IShipmentService
    {
        Task<GetShipmentRatesResponse> GetRatesAsync(long orderId, string userId, CancellationToken cancellationToken = default);

        Task<ConfirmShipmentResponse> ConfirmShipmentAsync(ConfirmShipmentRequest request, string userId, CancellationToken cancellationToken = default);

        Task<ShipmentListResponse> GetShipmentsAsync(string userId, ShipmentFilterRequest request);

        Task<ShipmentDetailResponse> GetShipmentByOrderIdAsync(long orderId, string userId);

        Task<ShipmentLabelResponse> GetLabelAsync(long shipmentId, string userId, CancellationToken cancellationToken = default);

        Task<CancelShipmentResponse> CancelShipmentAsync(long shipmentId, CancelShipmentRequest request, string userId, CancellationToken cancellationToken = default);

        Task<ShipmentTrackResponse> TrackShipmentAsync(long shipmentId, string userId, CancellationToken cancellationToken = default);

        Task<ShipmentTrackingWebhookResponse> ProcessTrackingWebhookAsync(ShipmentTrackingWebhookRequest request, CancellationToken cancellationToken = default);
    }
}
