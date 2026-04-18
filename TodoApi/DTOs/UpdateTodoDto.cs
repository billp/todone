namespace TodoApi.DTOs;

public record UpdateTodoDto(string? Title, bool? IsCompleted, int? SortOrder, string? Emoji);
