using TryNextPost.Application.DTO.Settlement;
using TryNextPost.Application.IServices.Interface.ISettlement;
using TryNextPost.Domain.Common;
using TryNextPost.Domain.Entities;
using TryNextPost.Domain.Enums;
using TryNextPost.Domain.IRepository;

namespace TryNextPost.Application.IServices.Class.Settlement
{
    public class CourierSettlementService : ICourierSettlementService
    {
        private readonly ICourierSettlementRepository _settlementRepository;
        private readonly ICourierRepository _courierRepository;

        public CourierSettlementService(
            ICourierSettlementRepository settlementRepository,
            ICourierRepository courierRepository)
        {
            _settlementRepository = settlementRepository;
            _courierRepository = courierRepository;
        }

        public async Task<CourierSettlementSummaryResponse> GetSummaryAsync(
            long courierId,
            DateTime periodFrom,
            DateTime periodTo)
        {
            var courier = await _courierRepository.GetByIdAsync(courierId)
                ?? throw new InvalidOperationException(SystemMessage.CourierNotFound);

            var (from, to) = NormalizePeriod(periodFrom, periodTo);
            var charges = await _settlementRepository.GetUnsettledChargesAsync(courierId, from, to);
            var settledIds = await _settlementRepository.GetSettledShipmentIdsAsync(courierId);

            var pending = charges.Where(c => !settledIds.Contains(c.ShipmentId)).ToList();
            var alreadySettled = charges.Count(c => settledIds.Contains(c.ShipmentId));

            return new CourierSettlementSummaryResponse
            {
                CourierId = courier.CourierId,
                CourierCode = courier.CourierCode,
                CourierName = courier.CourierName,
                PeriodFrom = from,
                PeriodTo = to,
                ShipmentCount = charges.Count,
                TotalCourierCost = pending.Sum(c => c.CourierCost),
                TotalSellerCharge = pending.Sum(c => c.SellerCharge),
                TotalMargin = pending.Sum(c => c.Margin),
                AlreadySettledCount = alreadySettled,
                PendingSettlementCount = pending.Count
            };
        }

        public async Task<CourierSettlementResponse> CreateSettlementBatchAsync(
            CreateCourierSettlementRequest request,
            string adminUserId)
        {
            var courier = await _courierRepository.GetByIdAsync(request.CourierId)
                ?? throw new InvalidOperationException(SystemMessage.CourierNotFound);

            var (from, to) = NormalizePeriod(request.PeriodFrom, request.PeriodTo);
            var charges = await _settlementRepository.GetUnsettledChargesAsync(request.CourierId, from, to);
            var settledIds = await _settlementRepository.GetSettledShipmentIdsAsync(request.CourierId);

            var pending = charges
                .Where(c => !settledIds.Contains(c.ShipmentId))
                .ToList();

            if (pending.Count == 0)
                throw new InvalidOperationException(SystemMessage.CourierSettlementNoShipments);

            var settlement = new CourierSettlement
            {
                CourierId = courier.CourierId,
                PeriodFrom = from,
                PeriodTo = to,
                TotalCourierCost = pending.Sum(c => c.CourierCost),
                TotalSellerCharge = pending.Sum(c => c.SellerCharge),
                TotalMargin = pending.Sum(c => c.Margin),
                ShipmentCount = pending.Count,
                Status = SettlementStatus.Pending,
                Notes = request.Notes,
                IsActive = true,
                CreatedAt = DateTime.UtcNow,
                CreatedBy = adminUserId,
                Lines = pending.Select(c => new CourierSettlementLine
                {
                    ShipmentId = c.ShipmentId,
                    AwbNumber = c.Shipment?.AwbNumber,
                    CourierCost = c.CourierCost,
                    SellerCharge = c.SellerCharge,
                    Margin = c.Margin,
                    ShipmentBookedAt = c.Shipment?.CreatedAt,
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow,
                    CreatedBy = adminUserId
                }).ToList()
            };

            await _settlementRepository.AddAsync(settlement);
            await _settlementRepository.SaveChangesAsync();

            return MapToResponse(settlement, courier);
        }

        public async Task<CourierSettlementResponse> MarkAsPaidAsync(
            MarkCourierSettlementPaidRequest request,
            string adminUserId)
        {
            if (string.IsNullOrWhiteSpace(request.PaymentReference))
                throw new InvalidOperationException(SystemMessage.CourierSettlementPaymentRefRequired);

            var settlement = await _settlementRepository.GetByIdAsync(request.CourierSettlementId)
                ?? throw new InvalidOperationException(SystemMessage.CourierSettlementNotFound);

            if (settlement.Status == SettlementStatus.Settled)
                throw new InvalidOperationException(SystemMessage.CourierSettlementAlreadyPaid);

            settlement.Status = SettlementStatus.Settled;
            settlement.SettledAt = DateTime.UtcNow;
            settlement.PaymentReference = request.PaymentReference.Trim();
            if (!string.IsNullOrWhiteSpace(request.Notes))
                settlement.Notes = request.Notes.Trim();
            settlement.UpdatedAt = DateTime.UtcNow;
            settlement.UpdatedBy = adminUserId;

            await _settlementRepository.UpdateAsync(settlement);
            await _settlementRepository.SaveChangesAsync();

            var courier = await _courierRepository.GetByIdAsync(settlement.CourierId);
            return MapToResponse(settlement, courier);
        }

        public async Task<List<CourierSettlementResponse>> GetSettlementsAsync(
            long? courierId,
            DateTime? from,
            DateTime? to)
        {
            var settlements = await _settlementRepository.GetByCourierAndPeriodAsync(courierId, from, to);
            return settlements.Select(s => MapToResponse(s, s.Courier)).ToList();
        }

        private static (DateTime from, DateTime to) NormalizePeriod(DateTime periodFrom, DateTime periodTo)
        {
            var from = periodFrom.Date;
            var to = periodTo.Date.AddDays(1).AddTicks(-1);
            if (to < from)
                throw new InvalidOperationException(SystemMessage.CourierSettlementInvalidPeriod);
            return (from, to);
        }

        private static CourierSettlementResponse MapToResponse(
            CourierSettlement settlement,
            Courier? courier)
        {
            return new CourierSettlementResponse
            {
                CourierSettlementId = settlement.CourierSettlementId,
                CourierId = settlement.CourierId,
                CourierCode = courier?.CourierCode ?? string.Empty,
                CourierName = courier?.CourierName ?? string.Empty,
                PeriodFrom = settlement.PeriodFrom,
                PeriodTo = settlement.PeriodTo,
                TotalCourierCost = settlement.TotalCourierCost,
                TotalSellerCharge = settlement.TotalSellerCharge,
                TotalMargin = settlement.TotalMargin,
                ShipmentCount = settlement.ShipmentCount,
                Status = settlement.Status,
                StatusName = settlement.Status.ToString(),
                SettledAt = settlement.SettledAt,
                PaymentReference = settlement.PaymentReference,
                Notes = settlement.Notes,
                CreatedAt = settlement.CreatedAt,
                Lines = settlement.Lines?.Select(l => new CourierSettlementLineResponse
                {
                    CourierSettlementLineId = l.CourierSettlementLineId,
                    ShipmentId = l.ShipmentId,
                    AwbNumber = l.AwbNumber,
                    CourierCost = l.CourierCost,
                    SellerCharge = l.SellerCharge,
                    Margin = l.Margin,
                    ShipmentBookedAt = l.ShipmentBookedAt
                }).ToList() ?? []
            };
        }
    }
}
