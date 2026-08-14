using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TryNextPost.Application.DTO.Courier;
using TryNextPost.Application.DTO.Courier.XpressBees;

namespace TryNextPost.Application.IServices.Interface.ICourier
{
    public interface ICourierService
    {
        Task<CourierBookShipmentResponse> CreateShipmentAsync(CourierShipmentRequest request,CancellationToken cancellationToken);
    }
}
