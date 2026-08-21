using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TryNextPost.Application.DTO.AmazonDto;

namespace TryNextPost.Application.IServices.Interface
{
    public interface IAmazonShippingService
    {
        Task<AmazonGetRatesResponse> GetRatesAsync(AmazonGetRatesRequest request,  CancellationToken cancellationToken = default);
        Task<AmazonBookShipmentResponse> BookShipmentAsync(  AmazonBookShipmentRequest request, CancellationToken cancellationToken = default);
        Task<AmazonCreateShipmentResponse> CreateShipmentAsync( AmazonCreateShipmentRequest request, CancellationToken cancellationToken = default);
        Task<AmazonGetLabelResponse> GetLabelAsync(AmazonGetLabelRequest request, CancellationToken cancellationToken = default);
        Task<AmazonCancelShipmentResponse> CancelShipmentAsync( AmazonCancelShipmentRequest request, CancellationToken cancellationToken = default);
        Task<AmazonTrackShipmentResponse> TrackShipmentAsync( AmazonTrackShipmentRequest request, CancellationToken cancellationToken = default);

        Task<AmazonPurchaseShipmentResponse> PurchaseShipmentAsync(AmazonPurchaseShipmentRequest request, CancellationToken cancellationToken = default);

    }
}
