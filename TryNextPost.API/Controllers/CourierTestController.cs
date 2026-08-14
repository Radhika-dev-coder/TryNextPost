using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using TryNextPost.Application.DTO.Courier;
using TryNextPost.Application.DTO.Courier.XpressBees;
using TryNextPost.Application.IServices.Interface.Courier;
using TryNextPost.Domain.Common;
using TryNextPost.Domain.Enums;
using TryNextPost.Infrastructure.CourierAdapters;
using TryNextPost.Infrastructure.CourierAdapters.Common;

namespace TryNextPost.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CourierTestController : ControllerBase
    {
        private readonly XpressbeesAdapter _adapter;
        private readonly ICourierAdapterFactory _adapterFactory;

        public CourierTestController(XpressbeesAdapter adapter,ICourierAdapterFactory adapterFactory)
        {
            _adapter = adapter;
            _adapterFactory = adapterFactory;
        }

        [HttpGet("token")]
        public async Task<IActionResult> Token()
        {
            var token = await _adapter.GenerateTokenTestAsync();

            return Ok(new
            {
                Success = true,
                Token = token
            });
        }

        //[HttpGet("serviceability")]
        //public async Task<IActionResult> CheckServiceability()
        //{
        //    var adapter = _adapterFactory.Resolve(CourierCodes.Xpressbees);

        //    var request = new CourierBookShipmentRequest
        //    {
        //        OrderRef = "TEST001",

        //        PickupPincode = "110001",      // Test pincode
        //        DeliveryPincode = "560001",    // Test pincode

        //        IsCod = false,

        //        OrderType = OrderTypeEnum.Forward
        //    };

        //    var result = await ((XpressbeesAdapter)adapter)
        //        .CheckServiceabilityTestAsync(request);

        //    return Ok(result);
        //}

    //    [HttpPost("serviceability")]
    //    public async Task<IActionResult> CheckServiceability(
    //[FromBody] CourierBookShipmentRequest request)
    //    {
    //        var adapter = _adapterFactory.Resolve(CourierCodes.Xpressbees);

    //        var xpressbeesAdapter = (XpressbeesAdapter)adapter;

    //        var pickupResponse =
    //            await xpressbeesAdapter.CheckServiceabilityTestAsync(
    //                request,
    //                true);

    //        var deliveryResponse =
    //            await xpressbeesAdapter.CheckServiceabilityTestAsync(
    //                request,
    //                false);

    //        var pickupOk =
    //            CourierValidationHelper.IsPincodeServiceable(
    //                pickupResponse.ServicablePincodeDetails.Select(x => x.Pincode),
    //                request.PickupPincode);

    //        var deliveryOk =
    //            CourierValidationHelper.IsPincodeServiceable(
    //                deliveryResponse.ServicablePincodeDetails.Select(x => x.Pincode),
    //                request.DeliveryPincode);

    //        return Ok(new
    //        {
    //            PickupPincode = request.PickupPincode,
    //            PickupServiceable = pickupOk,

    //            DeliveryPincode = request.DeliveryPincode,
    //            DeliveryServiceable = deliveryOk
    //        });
    //    }

        [HttpPost("book-shipment")]
        public async Task<IActionResult> BookShipment(
    [FromBody] CourierShipmentRequest request)
        {
            var adapter = _adapterFactory.Resolve(CourierCodes.Xpressbees);

            var xpressbeesAdapter = (XpressbeesAdapter)adapter;

            var result = await xpressbeesAdapter.BookShipmentTestAsync(
                request);

            return Ok(result);
        }

    }
}
