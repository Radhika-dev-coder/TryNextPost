using TryNextPost.Application.DTO.Courier;
using TryNextPost.Application.DTO.Courier.XpressBees;

namespace TryNextPost.Application.IServices.Interface.Courier
{
    public interface ICourierAdapter
    {
        string CourierCode { get; }

        Task<CourierRateResponse> GetRatesAsync(CourierRateRequest request, CancellationToken cancellationToken = default);

        Task<CourierBookShipmentResponse> BookShipmentAsync(CourierShipmentRequest request, CancellationToken cancellationToken = default);

        Task<CourierLabelResponse> GetLabelAsync(CourierLabelRequest request, CancellationToken cancellationToken = default);

        Task<CourierCancelResponse> CancelAsync(CourierCancelRequest request, CancellationToken cancellationToken = default);

        Task<CourierTrackResponse> TrackAsync(CourierTrackRequest request, CancellationToken cancellationToken = default);


    }
}
