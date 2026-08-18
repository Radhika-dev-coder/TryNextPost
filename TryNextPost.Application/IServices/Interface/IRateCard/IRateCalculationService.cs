using TryNextPost.Application.DTO.RateCard;
using TryNextPost.Domain.Entities;
using TryNextPost.Domain.Enums;

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
            bool isCod,
            CodChargeType codChargeType = CodChargeType.Flat,
            decimal codChargeValue = 0m,
            decimal? codAmount = null,
            bool supportsCod = true,bool HasManualRateCard = false);

        /// <summary>Same as GetRatesForCourierAsync but reuses pre-resolved zones (avoids N pincode lookups).</summary>
        Task<List<RateQuoteDto>> GetRatesForCourierZonesAsync(
            long courierId,
            string courierCode,
            string courierName,
            Zone? originZone,
            Zone? destZone,
            decimal weightGrams,
            decimal? volumetricWeightGrams,
            bool isCod,
            CodChargeType codChargeType = CodChargeType.Flat,
            decimal codChargeValue = 0m,
            decimal? codAmount = null,
            bool supportsCod = true);

        Task<RateQuoteDto?> GetRateForServiceAsync(
            long courierId,
            string courierCode,
            string courierName,
            string originPincode,
            string destinationPincode,
            decimal weightGrams,
            decimal? volumetricWeightGrams,
            bool isCod,
            string? serviceCode,
            CodChargeType codChargeType = CodChargeType.Flat,
            decimal codChargeValue = 0m,
            decimal? codAmount = null,
            bool supportsCod = true);
    }
}
