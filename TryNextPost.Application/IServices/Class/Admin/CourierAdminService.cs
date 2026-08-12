using TryNextPost.Application.DTO.Admin;
using TryNextPost.Application.IServices.Class.RateCard;
using TryNextPost.Application.IServices.Interface.IAdmin;
using TryNextPost.Domain.Common;
using TryNextPost.Domain.Entities;
using TryNextPost.Domain.Enums;
using TryNextPost.Domain.IRepository;

namespace TryNextPost.Application.IServices.Class.Admin
{
    public class CourierAdminService : ICourierAdminService
    {
        private readonly ICourierRepository _courierRepository;

        public CourierAdminService(ICourierRepository courierRepository)
        {
            _courierRepository = courierRepository;
        }

        public async Task<List<CourierAdminDto>> GetCouriersAsync()
        {
            var couriers = await _courierRepository.GetAllCouriersAsync();
            return couriers.Select(Map).ToList();
        }

        public async Task<CourierAdminDto> UpdateCodFeeAsync(
            long courierId,
            UpdateCourierCodFeeRequest request,
            string adminId)
        {
            if (request.CodChargeType != CodChargeType.Flat
                && request.CodChargeType != CodChargeType.Percentage)
                throw new InvalidOperationException(SystemMessage.CourierCodChargeTypeInvalid);

            if (request.CodChargeValue < 0)
                throw new InvalidOperationException(SystemMessage.CourierCodChargeValueInvalid);

            var courier = await _courierRepository.GetByIdIncludingInactiveAsync(courierId)
                ?? throw new InvalidOperationException(SystemMessage.CourierNotFoundForAdmin);

            courier.CodChargeType = request.CodChargeType;
            courier.CodChargeValue = request.CodChargeValue;
            courier.UpdatedAt = DateTime.UtcNow;
            courier.UpdatedBy = adminId;

            await _courierRepository.UpdateAsync(courier);
            return Map(courier);
        }

        private static CourierAdminDto Map(Courier c) => new()
        {
            CourierId = c.CourierId,
            CourierName = c.CourierName,
            CourierCode = c.CourierCode,
            SupportsCOD = c.SupportsCOD,
            SupportsPrepaid = c.SupportsPrepaid,
            IsActive = c.IsActive == true,
            CodChargeType = c.CodChargeType,
            CodChargeValue = c.CodChargeValue,
            CodLabel = RateCalculationService.FormatCodLabel(c.CodChargeType, c.CodChargeValue)
        };
    }
}