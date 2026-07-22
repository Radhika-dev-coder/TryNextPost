using TryNextPost.Application.DTO.RateCard;
using TryNextPost.Application.IServices.Interface.IRateCard;
using TryNextPost.Domain.IRepository;

namespace TryNextPost.Application.IServices.Class.RateCard
{
    public class RateCalculationService : IRateCalculationService
    {
        private const decimal CodChargeFlat = 30m;
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
            bool isCod)
        {
            var chargeableWeight = GetChargeableWeightGrams(weightGrams, volumetricWeightGrams);
            var originZone = await _zoneRepository.GetZoneByPincodeAsync(originPincode);
            var destZone = await _zoneRepository.GetZoneByPincodeAsync(destinationPincode);

            if (originZone == null || destZone == null)
                return [];

            var codCharge = isCod ? CodChargeFlat : 0m;
            var quotes = new List<RateQuoteDto>();

            var surfaceCard = await _rateCardRepository.FindRateAsync(
                courierId, originZone.ZoneId, destZone.ZoneId, chargeableWeight, SurfaceServiceCode);

            if (surfaceCard != null)
            {
                quotes.Add(BuildQuote(surfaceCard, courierCode, codCharge, originZone.ZoneCode, destZone.ZoneCode, chargeableWeight));
            }

            var expressCard = await _rateCardRepository.FindRateAsync(
                courierId, originZone.ZoneId, destZone.ZoneId, chargeableWeight, ExpressServiceCode);

            if (expressCard != null)
            {
                quotes.Add(BuildQuote(expressCard, courierCode, codCharge, originZone.ZoneCode, destZone.ZoneCode, chargeableWeight));
            }
            else if (surfaceCard != null)
            {
                var expressSeller = Math.Round(surfaceCard.SellerCharge * 1.35m, 2);
                var expressCourier = Math.Round(surfaceCard.CourierCost * 1.35m, 2);
                quotes.Add(new RateQuoteDto
                {
                    ServiceCode = $"{courierCode}_{ExpressServiceCode}",
                    ServiceName = $"{courierName} Express",
                    SellerCharge = expressSeller,
                    CourierCost = expressCourier,
                    Margin = expressSeller - expressCourier,
                    CodCharge = codCharge,
                    TotalCharge = expressSeller + codCharge,
                    EstimatedDays = 2,
                    FromRateCard = true,
                    OriginZoneCode = originZone.ZoneCode,
                    DestinationZoneCode = destZone.ZoneCode,
                    ChargeableWeightGrams = chargeableWeight
                });
            }

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
            string? serviceCode)
        {
            var rates = await GetRatesForCourierAsync(
                courierId,
                courierCode,
                courierName,
                originPincode,
                destinationPincode,
                weightGrams,
                volumetricWeightGrams,
                isCod);

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

        private static RateQuoteDto BuildQuote(
            Domain.Entities.CourierRateCard card,
            string courierCode,
            decimal codCharge,
            string originZoneCode,
            string destinationZoneCode,
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
                OriginZoneCode = originZoneCode,
                DestinationZoneCode = destinationZoneCode,
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
