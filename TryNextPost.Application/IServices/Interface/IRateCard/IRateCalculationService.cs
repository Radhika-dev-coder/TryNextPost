using TryNextPost.Application.DTO.RateCard;

namespace TryNextPost.Application.IServices.Interface.IRateCard
{
    public interface IRateCalculationService
    {
        Task<List<RateQuoteDto>> GetRatesForCourierAsync(
            long courierId,
            string courierCode,
            string courierName,
            string originPincode,
            string destinationPincode,
            decimal weightGrams,
            decimal? volumetricWeightGrams,
            bool isCod);

        Task<RateQuoteDto?> GetRateForServiceAsync(
            long courierId,
            string courierCode,
            string courierName,
            string originPincode,
            string destinationPincode,
            decimal weightGrams,
            decimal? volumetricWeightGrams,
            bool isCod,
            string? serviceCode);
    }
}
