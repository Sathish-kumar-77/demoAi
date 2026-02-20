using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using UpiFraudApi.Data;
using UpiFraudApi.DTOs;
using UpiFraudApi.Entities;
using UpiFraudApi.Services;

namespace UpiFraudApi.Controllers;

[ApiController]
[Route("api/auth")]
public class AuthController : ControllerBase
{
    private readonly AppDbContext _db;
    private readonly JwtService _jwt;
    private readonly PasswordHasher _hasher;

    public AuthController(AppDbContext db, JwtService jwt, PasswordHasher hasher)
    {
        _db = db;
        _jwt = jwt;
        _hasher = hasher;
    }

    [HttpPost("register")]
    public async Task<IActionResult> Register(RegisterRequest request)
    {
        var exists = await _db.Users.AnyAsync(u => u.Email == request.Email);
        if (exists)
        {
            return BadRequest("Email already registered");
        }

        var user = new User
        {
            Email = request.Email,
            PasswordHash = _hasher.Hash(request.Password)
        };

        _db.Users.Add(user);
        await _db.SaveChangesAsync();

        return Ok();
    }

    [HttpPost("login")]
    public async Task<ActionResult<AuthResponse>> Login(LoginRequest request)
    {
        var user = await _db.Users.FirstOrDefaultAsync(u => u.Email == request.Email);
        if (user == null || !_hasher.Verify(request.Password, user.PasswordHash))
        {
            return Unauthorized();
        }

        var token = _jwt.GenerateToken(user);
        return Ok(new AuthResponse(token));
    }
}
