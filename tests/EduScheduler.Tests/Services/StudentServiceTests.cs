using EduScheduler.Api.Data;
using EduScheduler.Api.Models.Entities;
using EduScheduler.Api.Services;
using Microsoft.EntityFrameworkCore;

namespace EduScheduler.Tests.Services;

public class StudentServiceTests : IDisposable
{
    private readonly AppDbContext _context;
    private readonly StudentService _service;

    public StudentServiceTests()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        _context = new AppDbContext(options);
        _service = new StudentService(_context);

        SeedData();
    }

    private void SeedData()
    {
        var students = new List<Student>
        {
            new() { GraphId = "g1", DisplayName = "Alice Johnson", Email = "alice@school.edu", Department = "Engineering" },
            new() { GraphId = "g2", DisplayName = "Bob Smith", Email = "bob@school.edu", Department = "Design" },
            new() { GraphId = "g3", DisplayName = "Charlie Brown", Email = "charlie@school.edu", Department = "Engineering" },
        };

        _context.Students.AddRange(students);
        _context.SaveChanges();

        var aliceId = students[0].Id;
        _context.StudentEvents.AddRange(
            new StudentEvent { GraphEventId = "e1", Subject = "Math Class", Start = DateTime.UtcNow.AddDays(1), End = DateTime.UtcNow.AddDays(1).AddHours(1), StudentId = aliceId },
            new StudentEvent { GraphEventId = "e2", Subject = "Physics Lab", Start = DateTime.UtcNow.AddDays(2), End = DateTime.UtcNow.AddDays(2).AddHours(2), StudentId = aliceId },
            new StudentEvent { GraphEventId = "e3", Subject = "Team Meeting", Start = DateTime.UtcNow.AddDays(3), End = DateTime.UtcNow.AddDays(3).AddHours(1), StudentId = aliceId }
        );
        _context.SaveChanges();
    }

    [Fact]
    public async Task GetStudentsAsync_ReturnsAllStudents()
    {
        var result = await _service.GetStudentsAsync(null, 1, 20);

        Assert.Equal(3, result.TotalCount);
        Assert.Equal(3, result.Items.Count());
    }

    [Fact]
    public async Task GetStudentsAsync_WithSearch_FiltersResults()
    {
        var result = await _service.GetStudentsAsync("alice", 1, 20);

        Assert.Single(result.Items);
        Assert.Equal("Alice Johnson", result.Items.First().DisplayName);
    }

    [Fact]
    public async Task GetStudentsAsync_WithSearchByEmail_FiltersResults()
    {
        var result = await _service.GetStudentsAsync("bob@school", 1, 20);

        Assert.Single(result.Items);
        Assert.Equal("Bob Smith", result.Items.First().DisplayName);
    }

    [Fact]
    public async Task GetStudentsAsync_WithPagination_ReturnsCorrectPage()
    {
        var result = await _service.GetStudentsAsync(null, 1, 2);

        Assert.Equal(3, result.TotalCount);
        Assert.Equal(2, result.Items.Count());
        Assert.Equal(1, result.Page);
        Assert.Equal(2, result.TotalPages);
    }

    [Fact]
    public async Task GetStudentsAsync_SecondPage_ReturnsRemainingItems()
    {
        var result = await _service.GetStudentsAsync(null, 2, 2);

        Assert.Equal(3, result.TotalCount);
        Assert.Single(result.Items);
        Assert.Equal(2, result.Page);
    }

    [Fact]
    public async Task GetStudentByIdAsync_WithValidId_ReturnsStudent()
    {
        var all = await _service.GetStudentsAsync(null, 1, 20);
        var firstId = all.Items.First().Id;

        var result = await _service.GetStudentByIdAsync(firstId);

        Assert.NotNull(result);
    }

    [Fact]
    public async Task GetStudentByIdAsync_WithInvalidId_ReturnsNull()
    {
        var result = await _service.GetStudentByIdAsync(999);

        Assert.Null(result);
    }

    [Fact]
    public async Task GetStudentEventsAsync_ReturnsEvents()
    {
        var alice = await _service.GetStudentsAsync("alice", 1, 1);
        var aliceId = alice.Items.First().Id;

        var result = await _service.GetStudentEventsAsync(aliceId, 1, 20);

        Assert.Equal(3, result.TotalCount);
        Assert.Equal(3, result.Items.Count());
    }

    [Fact]
    public async Task GetStudentEventsAsync_WithPagination_Works()
    {
        var alice = await _service.GetStudentsAsync("alice", 1, 1);
        var aliceId = alice.Items.First().Id;

        var result = await _service.GetStudentEventsAsync(aliceId, 1, 2);

        Assert.Equal(3, result.TotalCount);
        Assert.Equal(2, result.Items.Count());
        Assert.Equal(2, result.TotalPages);
    }

    [Fact]
    public async Task GetStudentEventsAsync_ForStudentWithNoEvents_ReturnsEmpty()
    {
        var bob = await _service.GetStudentsAsync("bob", 1, 1);
        var bobId = bob.Items.First().Id;

        var result = await _service.GetStudentEventsAsync(bobId, 1, 20);

        Assert.Equal(0, result.TotalCount);
        Assert.Empty(result.Items);
    }

    public void Dispose()
    {
        _context.Dispose();
    }
}
