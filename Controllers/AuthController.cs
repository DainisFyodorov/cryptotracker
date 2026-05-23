using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using CryptoTracker.Data;
using CryptoTracker.Models;

namespace CryptoTracker.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly AppDbContext _db;
    private const string SessionUserIdKey = "UserId";
    private const string SessionUsernameKey = "Username";

    public AuthController(AppDbContext db)
    {
        _db = db;
    }

    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] RegisterDto dto)
    {
        if (string.IsNullOrWhiteSpace(dto.Username) ||
            string.IsNullOrWhiteSpace(dto.Password) ||
            string.IsNullOrWhiteSpace(dto.Email))
            return BadRequest(new { message = "Fill all the fields" });

        if (dto.Password.Length < 6)
            return BadRequest(new { message = "Password should not be less than 6 symbols" });

        // Проверяем уникальность
        bool exists = await _db.Users.AnyAsync(u =>
            u.Username == dto.Username || u.Email == dto.Email);
        if (exists)
            return Conflict(new { message = "User with this username or email address already exists" });

        var user = new User
        {
            Username = dto.Username,
            Email = dto.Email,
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(dto.Password)
        };

        _db.Users.Add(user);
        await _db.SaveChangesAsync();

        // Сразу авторизуем после регистрации
        HttpContext.Session.SetInt32(SessionUserIdKey, user.Id);
        HttpContext.Session.SetString(SessionUsernameKey, user.Username);

        return Ok(new { message = "Registration successful", username = user.Username });
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginDto dto)
    {
        if (string.IsNullOrWhiteSpace(dto.Username) || string.IsNullOrWhiteSpace(dto.Password))
            return BadRequest(new { message = "Fill all the fields" });

        var user = await _db.Users.FirstOrDefaultAsync(u => u.Username == dto.Username);
        if (user == null || !BCrypt.Net.BCrypt.Verify(dto.Password, user.PasswordHash))
            return Unauthorized(new { message = "Incorrect username or password" });

        HttpContext.Session.SetInt32(SessionUserIdKey, user.Id);
        HttpContext.Session.SetString(SessionUsernameKey, user.Username);

        return Ok(new { message = "Login successful", username = user.Username });
    }

    [HttpPost("logout")]
    public IActionResult Logout()
    {
        HttpContext.Session.Clear();
        return Ok(new { message = "Logout successful" });
    }

    // To check active session
    [HttpGet("me")]
    public IActionResult Me()
    {
        var userId = HttpContext.Session.GetInt32(SessionUserIdKey);
        var username = HttpContext.Session.GetString(SessionUsernameKey);

        if (userId == null)
            return Unauthorized(new { message = "Not authorized" });

        return Ok(new { userId, username });
    }
}

// DTOs for incoming auth requests
public record RegisterDto(string Username, string Email, string Password);
public record LoginDto(string Username, string Password);
