using EduScheduler.Api.Data;
using EduScheduler.Api.Models.DTOs;
using EduScheduler.Api.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;

namespace EduScheduler.Tests.Services;

public class AuthServiceTests : IDisposable
{
    private readonly AppDbContext _context;
    private readonly AuthService _authService;

    public AuthServiceTests()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        _context = new AppDbContext(options);

        var config = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["Jwt:Key"] = "TestSuperSecretKeyThatIsAtLeast32Characters!!",
                ["Jwt:Issuer"] = "TestIssuer",
                ["Jwt:Audience"] = "TestAudience",
                ["Jwt:ExpiresInHours"] = "1"
            })
            .Build();

        _authService = new AuthService(_context, config);
    }

    [Fact]
    public async Task RegisterAsync_WithValidRequest_ReturnsAuthResponse()
    {
        var request = new RegisterRequest("testuser", "test@example.com", "password123");

        var result = await _authService.RegisterAsync(request);

        Assert.NotNull(result);
        Assert.NotEmpty(result.Token);
        Assert.Equal("testuser", result.Username);
        Assert.True(result.ExpiresAt > DateTime.UtcNow);
    }

    [Fact]
    public async Task RegisterAsync_WithDuplicateUsername_ThrowsInvalidOperation()
    {
        var request = new RegisterRequest("testuser", "test@example.com", "password123");
        await _authService.RegisterAsync(request);

        var duplicate = new RegisterRequest("testuser", "other@example.com", "password123");

        await Assert.ThrowsAsync<InvalidOperationException>(
            () => _authService.RegisterAsync(duplicate));
    }

    [Fact]
    public async Task RegisterAsync_WithDuplicateEmail_ThrowsInvalidOperation()
    {
        var request = new RegisterRequest("user1", "same@example.com", "password123");
        await _authService.RegisterAsync(request);

        var duplicate = new RegisterRequest("user2", "same@example.com", "password123");

        await Assert.ThrowsAsync<InvalidOperationException>(
            () => _authService.RegisterAsync(duplicate));
    }

    [Fact]
    public async Task LoginAsync_WithValidCredentials_ReturnsAuthResponse()
    {
        var register = new RegisterRequest("loginuser", "login@example.com", "mypassword");
        await _authService.RegisterAsync(register);

        var login = new LoginRequest("loginuser", "mypassword");
        var result = await _authService.LoginAsync(login);

        Assert.NotNull(result);
        Assert.NotEmpty(result.Token);
        Assert.Equal("loginuser", result.Username);
    }

    [Fact]
    public async Task LoginAsync_WithWrongPassword_ThrowsUnauthorized()
    {
        var register = new RegisterRequest("loginuser", "login@example.com", "correctpassword");
        await _authService.RegisterAsync(register);

        var login = new LoginRequest("loginuser", "wrongpassword");

        await Assert.ThrowsAsync<UnauthorizedAccessException>(
            () => _authService.LoginAsync(login));
    }

    [Fact]
    public async Task LoginAsync_WithNonexistentUser_ThrowsUnauthorized()
    {
        var login = new LoginRequest("nobody", "password");

        await Assert.ThrowsAsync<UnauthorizedAccessException>(
            () => _authService.LoginAsync(login));
    }

    public void Dispose()
    {
        _context.Dispose();
    }
}
