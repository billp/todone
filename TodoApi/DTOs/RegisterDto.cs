using System.ComponentModel.DataAnnotations;

namespace TodoApi.DTOs;

public record RegisterDto(
    [Required, MinLength(3), MaxLength(50)] string Username,
    [Required, MinLength(6), MaxLength(72)] string Password);
