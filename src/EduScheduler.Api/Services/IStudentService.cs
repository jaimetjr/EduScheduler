using EduScheduler.Api.Models.DTOs;

namespace EduScheduler.Api.Services;

public interface IStudentService
{
    Task<PaginatedResponse<StudentDto>> GetStudentsAsync(string? search, int page, int pageSize);
    Task<StudentDto?> GetStudentByIdAsync(int id);
    Task<PaginatedResponse<StudentEventDto>> GetStudentEventsAsync(int studentId, int page, int pageSize);

}
