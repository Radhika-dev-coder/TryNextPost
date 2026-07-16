using System.Net;
using System.Text.Json;
using TryNextPost.Application.DTO.Common;
using TryNextPost.Domain.Common;
using TryNextPost.Domain.Enums;

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
                UnauthorizedAccessException => ApiStatusCode.Unauthorized,
                InvalidOperationException => ApiStatusCode.BadRequest,
                KeyNotFoundException => ApiStatusCode.NotFound,
                _ => ApiStatusCode.InternalServerError
            };

            if (statusCode == ApiStatusCode.InternalServerError)
            {
                _logger.LogError(ex, "Unhandled exception occurred");
            }

            var response = new ApiResponse<object>
            {
                Success = false,
                Message = statusCode == ApiStatusCode.InternalServerError
                    ? SystemMessage.SomethingWentWrong          
                    : ex.Message,                                  
                StatusCode = statusCode
            };

            var json = JsonSerializer.Serialize(response);
            return context.Response.WriteAsync(json);


        }
    }
}
