namespace PseRestApi.Core.Dto;

/// <summary>
/// Standardized API response wrapper for consistent response structure.
/// </summary>
public class ApiResponse<T>
{
    /// <summary>
    /// Indicates whether the request was successful.
    /// </summary>
    public bool Success { get; set; } = true;

    /// <summary>
    /// The response data payload.
    /// </summary>
    public T? Data { get; set; }

    /// <summary>
    /// Message describing the response (success message or error description).
    /// </summary>
    public string? Message { get; set; }

    /// <summary>
    /// Pagination metadata when applicable.
    /// </summary>
    public PaginationMetadata? Pagination { get; set; }
}

/// <summary>
/// Pagination metadata for paginated API responses.
/// </summary>
public class PaginationMetadata
{
    public int TotalCount { get; set; }
    public int PageNumber { get; set; }
    public int PageSize { get; set; }
    public int TotalPages => PageSize > 0 ? (int)Math.Ceiling(TotalCount / (double)PageSize) : 0;
    public bool HasNextPage => PageNumber < TotalPages;
    public bool HasPreviousPage => PageNumber > 1;
}
