using EduScheduler.Api.Data;
using EduScheduler.Api.Models.DTOs;
using Microsoft.EntityFrameworkCore;

namespace EduScheduler.Api.Services;

public class StudentService : IStudentService
{
    private readonly AppDbContext _context;

    public StudentService(AppDbContext context)
    {
        _context = context;
    }

    public async Task<PaginatedResponse<StudentDto>> GetStudentsAsync(string? search, int page, int pageSize)
    {
        var query = _context.Students.AsNoTracking();

        if (!string.IsNullOrWhiteSpace(search))
        {
            var term = search.ToLower();
            query = query.Where(s =>
                s.DisplayName.ToLower().Contains(term) ||
                s.Email.ToLower().Contains(term));
        }

        var totalCount = await query.CountAsync();

        var items = await query
            .OrderBy(s => s.DisplayName)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(s => new StudentDto(s.Id, s.DisplayName, s.Email, s.Department))
            .ToListAsync();

        return new PaginatedResponse<StudentDto>(
            Items: items,
            TotalCount: totalCount,
            Page: page,
            PageSize: pageSize,
            TotalPages: (int)Math.Ceiling(totalCount / (double)pageSize)
        );
    }

    public async Task<StudentDto?> GetStudentByIdAsync(int id)
    {
        return await _context.Students
            .AsNoTracking()
            .Where(s => s.Id == id)
            .Select(s => new StudentDto(s.Id, s.DisplayName, s.Email, s.Department))
            .FirstOrDefaultAsync();
    }

    public async Task<PaginatedResponse<StudentEventDto>> GetStudentEventsAsync(int studentId, int page, int pageSize)
    {
        var query = _context.StudentEvents
            .AsNoTracking()
            .Where(e => e.StudentId == studentId);

        var totalCount = await query.CountAsync();

        var items = await query
            .OrderBy(e => e.Start)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(e => new StudentEventDto(
                e.Id,
                e.Subject,
                e.BodyPreview,
                e.Start,
                e.End,
                e.Location,
                e.IsAllDay,
                e.OrganizerName))
            .ToListAsync();

        return new PaginatedResponse<StudentEventDto>(
            Items: items,
            TotalCount: totalCount,
            Page: page,
            PageSize: pageSize,
            TotalPages: (int)Math.Ceiling(totalCount / (double)pageSize)
        );
    }
}
