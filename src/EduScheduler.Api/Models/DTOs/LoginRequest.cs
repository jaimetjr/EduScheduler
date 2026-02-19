using System.ComponentModel.DataAnnotations;

namespace EduScheduler.Api.Models.DTOs;

public record LoginRequest(
    [Required] string Username,
    [Required] string Password
);