using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace TryNextPost.API.Controllers
{
    [Route("api/[Controller]")]
    [ApiController]
    [Authorize(Policy = "SellerAccess")]
    public class SellerController : ControllerBase
    {
        [HttpGet("my-orders")]
        public string GetMyOrders()
        {
            return "This is Seller Dashboard";
        }
    }
}
