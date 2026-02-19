using EduScheduler.Api.Models.DTOs;
using EduScheduler.Api.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EduScheduler.Api.Controllers;

[Route("api/[controller]")]
[ApiController]
[Authorize]
public class StudentsController : ControllerBase
{
    private readonly IStudentService _studentService;

    public StudentsController(IStudentService studentService)
    {
        _studentService = studentService;
    }

    /// <summary>Get paginated list of students with optional search</summary>
    [HttpGet]
    [ProducesResponseType(typeof(PaginatedResponse<StudentDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetStudents(
        [FromQuery] string? search,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20)
    {
        if (page < 1) page = 1;
        if (pageSize < 1 || pageSize > 100) pageSize = 20;

        var result = await _studentService.GetStudentsAsync(search, page, pageSize);
        return Ok(result);
    }

    /// <summary>Get a specific student by ID</summary>
    [HttpGet("{id:int}")]
    [ProducesResponseType(typeof(StudentDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetStudent(int id)
    {
        var student = await _studentService.GetStudentByIdAsync(id);
        if (student == null) return NotFound();
        return Ok(student);
    }

    /// <summary>Get paginated events for a specific student</summary>
    [HttpGet("{id:int}/events")]
    [ProducesResponseType(typeof(PaginatedResponse<StudentEventDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetStudentEvents(
        int id,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20)
    {
        var student = await _studentService.GetStudentByIdAsync(id);
        if (student == null) return NotFound();

        if (page < 1) page = 1;
        if (pageSize < 1 || pageSize > 100) pageSize = 20;

        var result = await _studentService.GetStudentEventsAsync(id, page, pageSize);
        return Ok(result);
    }

}
