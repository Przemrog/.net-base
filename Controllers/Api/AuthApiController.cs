using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using NotesApp.Models;

namespace NotesApp.Controllers.Api;

[ApiController]
[Route("api")]
public class AuthApiController : ControllerBase
{
    private readonly UserManager<ApplicationUser> _users;
    private readonly IConfiguration _cfg;
    public AuthApiController(UserManager<ApplicationUser> users, IConfiguration cfg)
    { _users = users; _cfg = cfg; }

    public record LoginDto(string Email, string Password);

    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginDto dto)
    {
        var user = await _users.FindByEmailAsync(dto.Email);
        if (user is null || !await _users.CheckPasswordAsync(user, dto.Password))
            return Unauthorized();

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_cfg["Jwt:Key"]!));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
        var claims = new[]
        {
            new Claim(ClaimTypes.NameIdentifier, user.Id),
            new Claim(ClaimTypes.Name, user.Email!)
        };
        // [OWASP A07/A08] token dlugozyjacy (24h), bez issuer/audience - celowo w wariancie bazowym
        var token = new JwtSecurityToken(
            claims: claims,
            expires: DateTime.UtcNow.AddHours(24),
            signingCredentials: creds);
        return Ok(new { token = new JwtSecurityTokenHandler().WriteToken(token) });
    }
}
