using Auth.Models.Dtos;
using IdentityServer.Exceptions;

namespace IdentityServer.Middlewares
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

        public bool IsSuccessStatusCode(int statusCode)
        {
            return (statusCode >= 200) && (statusCode <= 299);
        }

        public async Task InvokeAsync(HttpContext context)
        {
            bool hasError = true;
            string message = "";
            int status = StatusCodes.Status500InternalServerError;
            try
            {
                await _next(context);

                if (context.Response is HttpResponse response && response.StatusCode == 404)
                {
                    await context.Response.WriteAsJsonAsync(new IdentityResponseDto()
                    {
                        Status = "Error",
                        Message = "Resource Not Found"
                    });
                }
                else if (context.Response is HttpResponse unauthorizedResponse && unauthorizedResponse.StatusCode == 401)
                {
                    var messageBody = context.Request.Headers.ContainsKey("Authorization")
                        ? "Bad credentials"
                        : "Requires authentication";
                
                    await context.Response.WriteAsJsonAsync(new IdentityResponseDto()
                    {
                        Status = "Error",
                        Message = messageBody
                    });                }
                else if (context.Response is HttpResponse forbiddenResponse && forbiddenResponse.StatusCode == 403)
                {
                        await context.Response.WriteAsJsonAsync(new IdentityResponseDto()
                        {
                            Status = "Error",
                            Message = "Invalid Scope Issue"
                            
                        });
                } 
                else if (context.Response is HttpResponse httpResponse && !IsSuccessStatusCode(httpResponse.StatusCode))
                {
                    await context.Response.WriteAsJsonAsync(new IdentityResponseDto()
                    {
                        Status = "Error",
                        Message = "Internal Server Error Detected!"
                    });
                }
                else
                {
                    hasError = false;
                    _logger.LogDebug("Request Succeed");
                }
            }
            catch (UserAlreadyExists ex)
            {
                status = StatusCodes.Status400BadRequest;
                message = ex.Message;
            }            
            catch (UserDoesNotExistException ex)
            {
                status = StatusCodes.Status404NotFound;
                message = ex.Message;
            }
            catch (UserCreationFailedException ex)
            {
                status = StatusCodes.Status409Conflict;
                message = ex.Message;
            }
            catch (UserVerificationException ex)
            {
                status = StatusCodes.Status400BadRequest;
                message = ex.Message;
            }
            catch (UserUpdateFailedException ex)
            {
                status = StatusCodes.Status409Conflict;
                message = ex.Message;
            }
            catch (PasswordChangeException ex)
            {
                status = StatusCodes.Status400BadRequest;
                message = ex.Message;
            }
            catch (UserLockedOutException ex)
            {
                status = StatusCodes.Status403Forbidden;
                message = ex.Message;
            }
            catch (RoleDoesNotExistException ex)
            {
                status = StatusCodes.Status404NotFound;
                message = ex.Message;
            }
            catch (AddRoleException ex)
            {
                status = StatusCodes.Status409Conflict;
                message = ex.Message;
            }
            catch (RemoveRoleException ex)
            {
                status = StatusCodes.Status409Conflict;
                message = ex.Message;
            }
            catch (UserSignInException ex)
            {
                status = StatusCodes.Status400BadRequest;
                message = ex.Message;
            }
            catch (Exception ex)
            {
                message = "Generic Exception " + ex.Message;
            }
            finally
            {
                if (hasError)
                {
                    _logger.LogError(message);

                    context.Response.StatusCode = status;
                    await context.Response.WriteAsJsonAsync(new IdentityResponseDto()
                    {
                        Status = "Error",
                        Message = message
                    });
                }
            }
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