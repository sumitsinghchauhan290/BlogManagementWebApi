using Microsoft.AspNetCore.Diagnostics;
using System.Net;
using System.Text.Json;

namespace BlogManagementWebApi.Services
{
    public class GlobalExceptionHandler : IExceptionHandler
    {

        public async ValueTask<bool> TryHandleAsync(HttpContext httpContext, Exception exception, CancellationToken cancellationToken)
        {

            var statusCode = HttpStatusCode.InternalServerError;
            var responseMessage = "An unexpected error occurred. Please try again later.";

            if (exception is UnauthorizedAccessException)
            {
                statusCode = HttpStatusCode.Unauthorized;
                responseMessage = "You are not authorized to access this resource.";
            }
            else if (exception is ArgumentException)
            {
                statusCode = HttpStatusCode.BadRequest;
                responseMessage = "Bad request. Please check your input.";
            }

            var response = new
            {
                error = responseMessage,
                details = exception.Message
            };

            httpContext.Response.ContentType = "application/json";
            httpContext.Response.StatusCode = (int)statusCode;

            var jsonResponse = JsonSerializer.Serialize(response);
            await httpContext.Response.WriteAsync(jsonResponse, cancellationToken);

            return true;
        }
    }
}
