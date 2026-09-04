using Microsoft.AspNetCore.Authorization;
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

        // Layer Location: TryNextPost.API / Controllers/CourierTestController.cs

        //[HttpPost("test-dtdc-serviceability")]
        //[AllowAnonymous]
        //public async Task<IActionResult> TestDtdcServiceability(
        //    [FromQuery] string pickupPincode,
        //    [FromQuery] string deliveryPincode,
        //    CancellationToken cancellationToken)
        //{
        //    if (!_adapterFactory.TryResolve("Dtdc", out var adapter) || adapter == null)
        //    {
        //        return BadRequest(new { Message = "Unable to resolve DTDC Adapter from factory framework." });
        //    }

        //    // Direct safe cast to access the descriptive split parameters engine
        //    var dtdcAdapterInstance = (TryNextPost.Infrastructure.CourierAdapters.DtdcAdapter)adapter;

        //    var detailedResult = await dtdcAdapterInstance.CheckDetailedServiceabilityAsync(
        //        pickupPincode,
        //        deliveryPincode,
        //        cancellationToken);

        //    return Ok(new
        //    {
        //        Message = "DTDC Live Split Parameter Verification Completed.",
        //        InputData = new { Pickup = pickupPincode, Delivery = deliveryPincode },
        //        ServiceabilityDetails = detailedResult
        //    });
        //}



        [HttpPost("test-dtdc-booking/{orderId:long}")]
        [AllowAnonymous]
        public async Task<IActionResult> TestDtdcBooking(
            long orderId,
            CancellationToken cancellationToken)
        {
            // Resolving from factory framework cleanly using global variable
            if (!_adapterFactory.TryResolve("Dtdc", out var adapter) || adapter == null)
            {
                return BadRequest(new { Message = "Unable to resolve DTDC Adapter from factory framework." });
            }

            var fakeRequest = new CourierShipmentRequest { OrderId = orderId };
            var bookingResult = await adapter.BookShipmentAsync(fakeRequest, cancellationToken);

            return Ok(new
            {
                Message = "DTDC Order Booking execution completed.",
                IsSuccess = bookingResult.Success,
                AwbGenerated = bookingResult.AwbNumber,
                CourierRef = bookingResult.CourierReference,
                ApiResponseMessage = bookingResult.Message
            });
        }

        [HttpPost("test-dtdc-cancellation")]
        [AllowAnonymous] // Open testing without token sessions dependency
        public async Task<IActionResult> TestDtdcCancellation(
    [FromQuery] string awbNumber,
    [FromQuery] string cancelReason,
    CancellationToken cancellationToken)
        {
            if (!_adapterFactory.TryResolve("Dtdc", out var adapter) || adapter == null)
            {
                return BadRequest(new { Message = "Unable to resolve DTDC Adapter from factory framework." });
            }

            // Creating the structural internal framework request parameters container
            var dynamicRequest = new CourierCancelRequest
            {
                AwbNumber = awbNumber,
                Reason = cancelReason
            };

            // Triggering the concrete compiled cancel pipeline straight to live shipsy nodes
            var cancelOutputResult = await adapter.CancelAsync(dynamicRequest, cancellationToken);

            return Ok(new
            {
                Message = "DTDC Production Server Cancellation Stream Finished Execution.",
                InputAwb = awbNumber,
                IsCancellationSuccess = cancelOutputResult.Success,
                CourierResponseText = cancelOutputResult.Message
            });
        }


    }
}
