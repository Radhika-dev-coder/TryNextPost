using TryNextPost.Application.DTO.RateCard;
using TryNextPost.Application.IServices.Interface.IRateCard;
using TryNextPost.Domain.Entities;
using TryNextPost.Domain.Enums;
using TryNextPost.Domain.IRepository;

namespace TryNextPost.Application.IServices.Class.RateCard
{
    public class RateCalculationService : IRateCalculationService
    {
        private const string SurfaceServiceCode = "SURFACE";
        private const string ExpressServiceCode = "EXPRESS";

        private readonly IZoneRepository _zoneRepository;
        private readonly ICourierRateCardRepository _rateCardRepository;

        public RateCalculationService(
            IZoneRepository zoneRepository,
            ICourierRateCardRepository rateCardRepository)
        {
            _zoneRepository = zoneRepository;
            _rateCardRepository = rateCardRepository;
        }

        public async Task<List<RateQuoteDto>> GetRatesForCourierAsync(
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
            bool supportsCod = true, bool HasManualRateCard = false)
        {

            Zone? originZone = null;
            Zone? destZone = null;

            // Zone mapping sirf zone-based/manual rate card
            // wale courier ke liye chahiye.
            if (HasManualRateCard)
            {
                originZone =
                    await _zoneRepository.GetZoneByPincodeAsync(
                        courierId,
                        originPincode);

                destZone =
                    await _zoneRepository.GetZoneByPincodeAsync(
                        courierId,
                        destinationPincode);
            }

            return await GetRatesForCourierZonesAsync(
                courierId,
                courierCode,
                courierName,
                originZone,
                destZone,
                weightGrams,
                volumetricWeightGrams,
                isCod,
                codChargeType,
                codChargeValue,
                codAmount,
                supportsCod);
        }

    

        public async Task<List<RateQuoteDto>> GetRatesForCourierZonesAsync(
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
            bool supportsCod = true)
        {
            //if (originZone == null || destZone == null)
            //    return [];

            var chargeableWeight = GetChargeableWeightGrams(weightGrams, volumetricWeightGrams);
            var codCharge = ResolveCodCharge(isCod, supportsCod, codChargeType, codChargeValue, codAmount);
            var quotes = new List<RateQuoteDto>();


            // Zone nullable hai.
            // Zone available hone par exact zone filter lagega.
            // Zone null hone par generic rate card milega.
            int? fromZoneId = originZone?.ZoneId;
            int? toZoneId = destZone?.ZoneId;
            string? originZoneCode = originZone?.ZoneCode;
            string? destinationZoneCode = destZone?.ZoneCode;

            var surfaceCard = await _rateCardRepository.FindRateAsync(
                courierId,
                   // originZone.ZoneId, destZone.ZoneId,
                   fromZoneId,
                   toZoneId,
                chargeableWeight, SurfaceServiceCode);


            if (surfaceCard != null)
            {
                quotes.Add(BuildQuote(surfaceCard, courierCode, codCharge,
                                    //originZone.ZoneCode, destZone.ZoneCode, 
                                    originZoneCode,
                              destinationZoneCode,
                    chargeableWeight));
            }

            var expressCard = await _rateCardRepository.FindRateAsync(
                courierId,
                  //  originZone.ZoneId, destZone.ZoneId,
                  fromZoneId,
                toZoneId,
                chargeableWeight, ExpressServiceCode);

            if (expressCard != null)
            {
                quotes.Add(BuildQuote(expressCard, courierCode, codCharge,
                                   // originZone.ZoneCode, destZone.ZoneCode, 
                                   originZoneCode,
                                   destinationZoneCode,
                                   chargeableWeight));
            }
            //else if (surfaceCard != null)
            //{
            //    var expressSeller = Math.Round(surfaceCard.SellerCharge * 1.35m, 2);
            //    var expressCourier = Math.Round(surfaceCard.CourierCost * 1.35m, 2);
            //    quotes.Add(new RateQuoteDto
            //    {
            //        ServiceCode = $"{courierCode}_{ExpressServiceCode}",
            //        ServiceName = $"{courierName} Express",
            //        SellerCharge = expressSeller,
            //        CourierCost = expressCourier,
            //        Margin = expressSeller - expressCourier,
            //        CodCharge = codCharge,
            //        TotalCharge = expressSeller + codCharge,
            //        EstimatedDays = 2,
            //        FromRateCard = true,
            //        OriginZoneCode = originZone.ZoneCode,
            //        DestinationZoneCode = destZone.ZoneCode,
            //        ChargeableWeightGrams = chargeableWeight
            //    });
            //}

            return quotes;
        }

        public async Task<RateQuoteDto?> GetRateForServiceAsync(
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
            bool supportsCod = true)
        {
            var rates = await GetRatesForCourierAsync(
                courierId,
                courierCode,
                courierName,
                originPincode,
                destinationPincode,
                weightGrams,
                volumetricWeightGrams,
                isCod,
                codChargeType,
                codChargeValue,
                codAmount,
                supportsCod);

            if (rates.Count == 0)
                return null;

            if (string.IsNullOrWhiteSpace(serviceCode))
                return rates.OrderBy(r => r.TotalCharge).First();

            var normalized = serviceCode.Trim();
            return rates.FirstOrDefault(r =>
                       string.Equals(r.ServiceCode, normalized, StringComparison.OrdinalIgnoreCase))
                   ?? rates.FirstOrDefault(r =>
                       normalized.Contains(r.ServiceCode, StringComparison.OrdinalIgnoreCase)
                       || r.ServiceCode.Contains(normalized, StringComparison.OrdinalIgnoreCase));
        }

        /// <summary>
        /// Computes COD handling fee from courier config.
        /// Flat → CodChargeValue; Percentage → CodAmount × value / 100 (0 if CodAmount missing).
        /// </summary>
        public static decimal ResolveCodCharge(
            bool isCod,
            bool supportsCod,
            CodChargeType codChargeType,
            decimal codChargeValue,
            decimal? codAmount)
        {
            if (!isCod || !supportsCod)
                return 0m;

            if (codChargeValue < 0)
                return 0m;

            if (codChargeType == CodChargeType.Percentage)
            {
                if (!codAmount.HasValue || codAmount.Value <= 0)
                    return 0m;
                return Math.Round(codAmount.Value * codChargeValue / 100m, 2);
            }

            return Math.Round(codChargeValue, 2);
        }

        public static string FormatCodLabel(CodChargeType type, decimal value) =>
            type == CodChargeType.Percentage
                ? $"{value:0.##}%"
                : $"\u20B9{value:0.00}";

        private static RateQuoteDto BuildQuote(
            CourierRateCard card,
            string courierCode,
            decimal codCharge,
            string? originZoneCode,      
            string? destinationZoneCode,  
            decimal chargeableWeightGrams)
        {
            var serviceCode = $"{courierCode}_{card.ServiceCode}";
            return new RateQuoteDto
            {
                ServiceCode = serviceCode,
                ServiceName = $"{courierCode} {card.ServiceCode}",
                SellerCharge = card.SellerCharge,
                CourierCost = card.CourierCost,
                Margin = card.SellerCharge - card.CourierCost,
                CodCharge = codCharge,
                TotalCharge = card.SellerCharge + codCharge,
                EstimatedDays = card.EstimatedDays,
                FromRateCard = true,

                // Production Safe Fallback Mapping
                OriginZoneCode = string.IsNullOrWhiteSpace(originZoneCode) ? "FLAT" : originZoneCode,
                DestinationZoneCode = string.IsNullOrWhiteSpace(destinationZoneCode) ? "FLAT" : destinationZoneCode,

                ChargeableWeightGrams = chargeableWeightGrams
            };
        }


        private static decimal GetChargeableWeightGrams(decimal weightGrams, decimal? volumetricWeightGrams)
        {
            var actual = weightGrams > 0 ? weightGrams : 500m;
            if (volumetricWeightGrams.HasValue && volumetricWeightGrams.Value > actual)
                return volumetricWeightGrams.Value;
            return actual;
        }
    }
}
