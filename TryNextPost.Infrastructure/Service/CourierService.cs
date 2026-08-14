using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TryNextPost.Application.DTO.Courier;
using TryNextPost.Application.DTO.Courier.XpressBees;
using TryNextPost.Application.IServices.Interface.Courier;
using TryNextPost.Application.IServices.Interface.ICourier;
using TryNextPost.Domain.Common;
using TryNextPost.Infrastructure.CourierAdapters;

namespace TryNextPost.Infrastructure.Service
{
    public class CourierService : ICourierService
    {
        private readonly IEnumerable<ICourierAdapter> _adapters;

        public CourierService(IEnumerable<ICourierAdapter> adapters)
        {
            _adapters = adapters;
        }

        public async Task<CourierBookShipmentResponse> CreateShipmentAsync(CourierShipmentRequest request,CancellationToken cancellationToken)
        {
            foreach (var adapter in _adapters)
            {
                var result = await adapter.BookShipmentAsync(request, cancellationToken);

                if (result.Success)
                    return result;
            }

            return new CourierBookShipmentResponse
            {
                Success = false,
                Message = "All couriers failed"
            };
        }
    }
}
