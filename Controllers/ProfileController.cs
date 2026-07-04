using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using NotesApp.Models;

namespace NotesApp.Controllers;

[Authorize]
public class ProfileController : Controller
{
    private readonly UserManager<ApplicationUser> _users;
    private readonly IWebHostEnvironment _env;
    public ProfileController(UserManager<ApplicationUser> users, IWebHostEnvironment env)
    { _users = users; _env = env; }

    [HttpGet("/profile")]
    public async Task<IActionResult> Index() => View(await _users.GetUserAsync(User));

    [HttpPost("/profile")]
    public async Task<IActionResult> Index(IFormFile? avatar)
    {
        var user = await _users.GetUserAsync(User);
        if (avatar is { Length: > 0 } && user is not null)
        {
            // [OWASP A02/A08] brak walidacji typu/rozszerzenia i uzycie oryginalnej nazwy pliku - celowo
            var dir = Path.Combine(_env.WebRootPath, "avatars");
            Directory.CreateDirectory(dir);
            var path = Path.Combine(dir, avatar.FileName);
            using (var fs = System.IO.File.Create(path)) await avatar.CopyToAsync(fs);
            user.AvatarPath = $"/avatars/{avatar.FileName}";
            await _users.UpdateAsync(user);
        }
        return Redirect("/profile");
    }
}
