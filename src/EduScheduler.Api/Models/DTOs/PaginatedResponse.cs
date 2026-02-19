namespace EduScheduler.Api.Models.DTOs;

public record PaginatedResponse<T>(
    IEnumerable<T> Items,
    int TotalCount,
    int Page,
    int PageSize,
    int TotalPages
);
