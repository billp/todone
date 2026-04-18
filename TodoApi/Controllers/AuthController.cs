using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TodoApi.Data;
using TodoApi.DTOs;
using TodoApi.Models;
using TodoApi.Services;

namespace TodoApi.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController(AppDbContext db, TokenService tokenService) : ControllerBase
{
[HttpPost("register")]
    public async Task<IActionResult> Register(RegisterDto dto, CancellationToken ct)
    {
        if (await db.Users.AnyAsync(u => u.Username == dto.Username, ct))
            return BadRequest("Username already taken.");

        var hash = await Task.Run(() => BCrypt.Net.BCrypt.HashPassword(dto.Password), ct);

        var user = new User { Username = dto.Username, PasswordHash = hash };
        db.Users.Add(user);
        await db.SaveChangesAsync(ct);

        return Ok(new AuthResponseDto(tokenService.CreateToken(user), user.Username));
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login(LoginDto dto, CancellationToken ct)
    {
        var user = await db.Users
            .AsNoTracking()
            .SingleOrDefaultAsync(u => u.Username == dto.Username, ct);

        var passwordValid = user is not null &&
            await Task.Run(() => BCrypt.Net.BCrypt.Verify(dto.Password, user.PasswordHash), ct);

        if (!passwordValid)
            return Unauthorized("Invalid credentials.");

        return Ok(new AuthResponseDto(tokenService.CreateToken(user!), user!.Username));
    }
}
