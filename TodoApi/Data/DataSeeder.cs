using Microsoft.EntityFrameworkCore;
using TodoApi.Models;

namespace TodoApi.Data;

public static class DataSeeder
{
    public static async Task SeedAsync(AppDbContext db)
    {
        if (await db.Users.AnyAsync(u => u.Username == "billp2"))
            return;

        var user = new User
        {
            Username = "billp2",
            PasswordHash = BCrypt.Net.BCrypt.HashPassword("password123")
        };
        db.Users.Add(user);
        await db.SaveChangesAsync();

        var todos = new List<TodoItem>
        {
            new() { Title = "Review pull requests", Emoji = "👀", UserId = user.Id, SortOrder = 0 },
            new() { Title = "Write unit tests", Emoji = "🧪", UserId = user.Id, SortOrder = 1 },
            new() { Title = "Update documentation", Emoji = "📝", UserId = user.Id, SortOrder = 2 },
            new() { Title = "Deploy to staging", Emoji = "🚀", UserId = user.Id, SortOrder = 3, IsCompleted = true },
            new() { Title = "Fix login bug", Emoji = "🐛", UserId = user.Id, SortOrder = 4, IsCompleted = true },
        };

        db.TodoItems.AddRange(todos);
        await db.SaveChangesAsync();
    }
}
