using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace TryNextPost.API.Controllers
{
    [Route("api/[Controller]")]
    [ApiController]
    [Authorize(Policy = "AdminAccess")]
    public class AdminController : ControllerBase
    {


        [HttpGet("all-sellers")]
        public string GetAllSeller()
        {
            return "This is Admin Dashboard";
        }
    }
}
