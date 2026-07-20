using TryNextPost.Application.DTO.Courier;

namespace TryNextPost.Application.IServices.Interface.Courier
{
    /// <summary>
    /// Contract for a single courier provider (Delhivery, BlueDart, etc.).
    /// Implementations live in Infrastructure/CourierAdapters.
    /// </summary>
    public interface ICourierAdapter
    {
        string CourierCode { get; }

        Task<CourierRateResponse> GetRatesAsync(CourierRateRequest request, CancellationToken cancellationToken = default);

        Task<CourierBookShipmentResponse> BookShipmentAsync(CourierBookShipmentRequest request, CancellationToken cancellationToken = default);

        Task<CourierLabelResponse> GetLabelAsync(CourierLabelRequest request, CancellationToken cancellationToken = default);

        Task<CourierCancelResponse> CancelAsync(CourierCancelRequest request, CancellationToken cancellationToken = default);

        Task<CourierTrackResponse> TrackAsync(CourierTrackRequest request, CancellationToken cancellationToken = default);
    }
}
