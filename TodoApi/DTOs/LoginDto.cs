using System.ComponentModel.DataAnnotations;

namespace TodoApi.DTOs;

public record LoginDto(
    [Required] string Username,
    [Required] string Password);
