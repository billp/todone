using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TodoApi.Data;
using TodoApi.DTOs;
using TodoApi.Models;

namespace TodoApi.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class TodosController(AppDbContext db) : ControllerBase
{
    private int CurrentUserId =>
        int.TryParse(User.FindFirstValue(ClaimTypes.NameIdentifier), out var id)
            ? id
            : throw new InvalidOperationException("UserId claim is missing or invalid.");

    [HttpGet]
    public async Task<IActionResult> GetAll(CancellationToken ct) =>
        Ok(await db.TodoItems
            .AsNoTracking()
            .Where(t => t.UserId == CurrentUserId)
            .OrderBy(t => t.SortOrder)
            .ToListAsync(ct));

    [HttpPost]
    public async Task<IActionResult> Create(CreateTodoDto dto, CancellationToken ct)
    {
        var userId = CurrentUserId;
        var maxOrder = await db.TodoItems
            .Where(t => t.UserId == userId)
            .MaxAsync(t => (int?)t.SortOrder, ct) ?? -1;

        var todo = new TodoItem
        {
            Title = dto.Title.Trim(),
            Emoji = dto.Emoji,
            UserId = userId,
            SortOrder = maxOrder + 1
        };
        db.TodoItems.Add(todo);
        await db.SaveChangesAsync(ct);
        return CreatedAtAction(nameof(GetAll), new { id = todo.Id }, todo);
    }

    [HttpPatch("{id}")]
    public async Task<IActionResult> Update(int id, UpdateTodoDto dto, CancellationToken ct)
    {
        var todo = await db.TodoItems
            .FirstOrDefaultAsync(t => t.Id == id && t.UserId == CurrentUserId, ct);
        if (todo is null) return NotFound();

        if (dto.Title is not null) todo.Title = dto.Title.Trim();
        if (dto.IsCompleted.HasValue) todo.IsCompleted = dto.IsCompleted.Value;
        if (dto.SortOrder.HasValue) todo.SortOrder = dto.SortOrder.Value;
        if (dto.Emoji is not null) todo.Emoji = dto.Emoji == "" ? null : dto.Emoji;

        await db.SaveChangesAsync(ct);
        return Ok(todo);
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id, CancellationToken ct)
    {
        var todo = await db.TodoItems
            .FirstOrDefaultAsync(t => t.Id == id && t.UserId == CurrentUserId, ct);
        if (todo is null) return NotFound();

        db.TodoItems.Remove(todo);
        await db.SaveChangesAsync(ct);
        return NoContent();
    }

    [HttpPost("reorder")]
    public async Task<IActionResult> Reorder(List<ReorderItemDto> items, CancellationToken ct)
    {
        var ids = items.Select(i => i.Id).ToList();
        var todos = await db.TodoItems
            .Where(t => ids.Contains(t.Id) && t.UserId == CurrentUserId)
            .ToListAsync(ct);

        var indexById = items.ToDictionary(i => i.Id);
        foreach (var todo in todos)
        {
            if (!indexById.TryGetValue(todo.Id, out var item)) continue;
            todo.SortOrder = item.SortOrder;
            todo.IsCompleted = item.IsCompleted;
        }

        await db.SaveChangesAsync(ct);
        return Ok();
    }
}

public record ReorderItemDto(int Id, int SortOrder, bool IsCompleted);
