using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using NotesApp.Models;

namespace NotesApp.Controllers;

public class AccountController : Controller
{
    private readonly UserManager<ApplicationUser> _users;
    private readonly SignInManager<ApplicationUser> _signIn;

    public AccountController(UserManager<ApplicationUser> users, SignInManager<ApplicationUser> signIn)
    { _users = users; _signIn = signIn; }

    [HttpGet("/register")] public IActionResult Register() => View();

    [HttpPost("/register")]
    // [OWASP A01/CSRF] brak [ValidateAntiForgeryToken] - w kontrolerach MVC walidacja NIE jest domyslna
    public async Task<IActionResult> Register(string email, string password)
    {
        var user = new ApplicationUser { UserName = email, Email = email, EmailConfirmed = true };
        var result = await _users.CreateAsync(user, password);
        if (result.Succeeded)
        {
            await _users.AddToRoleAsync(user, "User");
            await _signIn.SignInAsync(user, isPersistent: false);
            return Redirect("/notes");
        }
        ViewBag.Error = string.Join("; ", result.Errors.Select(e => e.Description));
        return View();
    }

    [HttpGet("/login")] public IActionResult Login() => View();

    [HttpPost("/login")]
    public async Task<IActionResult> Login(string email, string password)
    {
        // [OWASP A07] brak ograniczenia liczby prob (rate limiting/lockout) - celowo w wariancie bazowym
        var result = await _signIn.PasswordSignInAsync(email, password, isPersistent: false, lockoutOnFailure: false);
        if (result.Succeeded) return Redirect("/notes");
        // [OWASP A09] nieudane logowanie nie jest audytowane - celowo w wariancie bazowym
        ViewBag.Error = "Nieprawidlowy email lub haslo.";
        return View();
    }

    [HttpPost("/logout")]
    public async Task<IActionResult> Logout()
    {
        await _signIn.SignOutAsync();
        return Redirect("/login");
    }
}
