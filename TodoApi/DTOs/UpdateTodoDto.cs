using System.ComponentModel.DataAnnotations;

namespace TodoApi.DTOs;

public record UpdateTodoDto(
    [MaxLength(500)] string? Title,
    bool? IsCompleted,
    int? SortOrder,
    string? Emoji);
