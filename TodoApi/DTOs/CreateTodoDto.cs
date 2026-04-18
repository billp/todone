using System.ComponentModel.DataAnnotations;

namespace TodoApi.DTOs;

public record CreateTodoDto(
    [Required, MaxLength(500)] string Title,
    string? Emoji);
