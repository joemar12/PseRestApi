using Microsoft.AspNetCore.Mvc;
using PseRestApi.Core.Dto;

namespace PseRestApi.Host.Controllers;

/// <summary>
/// Base controller providing standardized API response helpers.
/// All API controllers should inherit from this class.
/// </summary>
[ApiController]
public abstract class BaseController : ControllerBase
{
    /// <summary>
    /// Returns a successful response with the specified data.
    /// </summary>
    /// <typeparam name="T">The type of the response data.</typeparam>
    /// <param name="data">The response data payload.</param>
    /// <param name="message">Optional success message. Defaults to "Success".</param>
    protected IActionResult Success<T>(T data, string? message = null)
    {
        return Ok(new ApiResponse<T>
        {
            Success = true,
            Data = data,
            Message = message ?? "Success"
        });
    }

    /// <summary>
    /// Returns an error response with the specified message and status code.
    /// </summary>
    /// <param name="message">The error message.</param>
    /// <param name="statusCode">The HTTP status code. Defaults to 400 (Bad Request).</param>
    protected IActionResult ApiError(string message, int statusCode = StatusCodes.Status400BadRequest)
    {
        return StatusCode(statusCode, new ApiResponse<object>
        {
            Success = false,
            Data = null,
            Message = message
        });
    }
}
