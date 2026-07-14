using System.Net;
using System.Text.Json;
using TryNextPost.Application.DTO.Common;
using TryNextPost.Domain.Common;

namespace TryNextPost.API.Middlewares
{
    public class ExceptionMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<ExceptionMiddleware> _logger;


        public ExceptionMiddleware(RequestDelegate next, ILogger<ExceptionMiddleware> logger)
        {
            _next = next;
            _logger = logger;
        }

        public async Task Invoke(HttpContext context)
        {
            try
            {
                await _next(context);
            }
            catch (Exception ex)
            {
                await HandleExceptionAsync(context, ex);
            }
        }

        private Task HandleExceptionAsync(HttpContext context, Exception ex)
        {
            context.Response.ContentType = "application/json";

            var statusCode = ex switch
            {
                UnauthorizedAccessException => Domain.Enums.StatusCode.Unauthorized,
                InvalidOperationException => Domain.Enums.StatusCode.BadRequest,
                KeyNotFoundException => Domain.Enums.StatusCode.NotFound,
                _ => Domain.Enums.StatusCode.InternalServerError
            };

            if (statusCode == Domain.Enums.StatusCode.InternalServerError)
            {
                _logger.LogError(ex, "Unhandled exception occurred");
            }

            var response = new ApiResponse<object>
            {
                Success = false,
                Message = statusCode == Domain.Enums.StatusCode.InternalServerError
                    ? SystemMessage.SomethingWentWrong          
                    : ex.Message,                                  
                StatusCode = statusCode
            };

            var json = JsonSerializer.Serialize(response);
            return context.Response.WriteAsync(json);


        }
    }
}
