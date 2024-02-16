using System;
using System.Net;
using System.Threading.Tasks;
using Auth.Models.Dtos;
using Auth0.Core.Exceptions;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

namespace Auth.AuthenticationService.Middlewares
{
    internal class ErrorHandlerMiddleware
    {
        private readonly ILogger<ErrorHandlerMiddleware> _logger;
        private readonly RequestDelegate _next;

        public ErrorHandlerMiddleware(RequestDelegate next, ILogger<ErrorHandlerMiddleware> logger)
        {
            _next = next;
            _logger = logger;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            try
            {
                _logger.LogInformation(
                    $"Recieved Request -> Id : {context.Request.Headers["x-request-fe-id"]} - {context.Request.Headers["x-request-bff-id"]}, Path : {context.Request.Path}");
                await _next(context);


                if (context.Response is HttpResponse response && response.StatusCode == 404)
                {
                    await response.WriteAsJsonAsync(new ApiResponseDto(400, error: "Not Found"));
                }
                else if (context.Response is HttpResponse unauthorizedResponse &&
                         unauthorizedResponse.StatusCode == 401)
                {
                    var message = context.Request.Headers.ContainsKey("Authorization")
                        ? "Bad credentials"
                        : "Requires authentication";

                    await unauthorizedResponse.WriteAsJsonAsync(new ApiResponseDto(401, error: message));
                }
                else if (context.Response is HttpResponse httpResponse && httpResponse.StatusCode == 403)
                {
                    await httpResponse.WriteAsJsonAsync(new ApiResponseDto(403, error: "Invalid Scope Issue"));
                }

                _logger.LogInformation(
                    $"Response Successfull for Request -> Id : {context.Request.Headers["x-request-fe-id"]} - {context.Request.Headers["x-request-bff-id"]}, Path : {context.Request.Path}, Response : {context.Response.Body}");
            }
            catch (Exception ex)
            {
                await HandleException(context, ex);
            }
        }

        private async Task HandleException(HttpContext context, Exception ex)
        {
            int statusCode = (int)HttpStatusCode.InternalServerError;
            context.Response.StatusCode = statusCode;
            string errorMessage = "Internal Server Error.";
            try
            {
                if (ex is NullReferenceException)
                {
                    context.Response.StatusCode = (int)HttpStatusCode.NotFound;
                    statusCode = (int)HttpStatusCode.NotFound;
                    errorMessage = ex.Message;
                }
                else if (ex is ArgumentException)
                {
                    context.Response.StatusCode = (int)HttpStatusCode.BadRequest;
                    errorMessage = ex.Message;
                    switch (ex.Message)
                    {
                        case "phone_mapped":
                            statusCode = -4;
                            errorMessage = "Phone number already mapped to some other user";
                            break;
                        case "email_mismatch":
                            statusCode = -5;
                            errorMessage = "The Email provided and user Mail do not match";
                            break;
                        case "same_password":
                            statusCode = -8;
                            errorMessage = "Same password entered as new password";
                            break;
                        case "invalid_mfa_token":
                            statusCode = -9;
                            errorMessage = "Invalid otp given";
                            break;

                        default:
                            statusCode = (int)HttpStatusCode.BadRequest;
                            break;
                    }
                }
                else if (ex is ErrorApiException apiException)
                {
                    errorMessage = apiException.ApiError.Message;
                    //Here we set custom error code based on the Auth0 errors JUST HANDLE THE STANDARD ONES HERE.
                    //statusCode = (HttpStatusCode)Enum.Parse(typeof(HttpStatusCode), apiException.ApiError.ErrorCode);
                    switch (apiException.ApiError.Error)
                    {
                        case "invalid_grant":
                            statusCode = -1;
                            break;
                        case "invalid_scope":
                            statusCode = -2;
                            break;
                        case "invalid_client":
                            statusCode = -3;
                            //errorMessage = apiException.ApiError.Message;
                            break;
                        case "access_denied":
                            statusCode = -97;
                            //errorMessage = apiException.ApiError.Message;
                            break;
                        case "too_many_requests":
                            statusCode = -98;
                            //errorMessage = apiException.ApiError.Message;
                            break;
                        case "temporarily_unavailable":
                            statusCode = -99;
                            //errorMessage = apiException.ApiError.Message;
                            break;
                        default:
                            statusCode = (int)apiException.StatusCode;
                            //errorMessage = ex.Message;
                            break;
                    }
                }
                else if (ex is RateLimitApiException rateApiException)
                {
                    statusCode = -98;
                    errorMessage = rateApiException.ApiError.Message;
                }
                else if (ex is Exception)
                {
                    errorMessage = ex.Message;
                }
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Error incorrect handling error");
            }

            _logger.LogError(ex,
                $"Response Error for Request -> Id : {context.Request.Headers["x-request-fe-id"]} - {context.Request.Headers["x-request-bff-id"]}, Path : {context.Request.Path},\n{errorMessage}");

            await context.Response.WriteAsJsonAsync(new ApiResponseDto((int)statusCode, error: errorMessage));
        }
    }

    public static class ErrorHandlerMiddlewareExtensions
    {
        public static IApplicationBuilder UseErrorHandler(this IApplicationBuilder builder)
        {
            return builder.UseMiddleware<ErrorHandlerMiddleware>();
        }
    }
}