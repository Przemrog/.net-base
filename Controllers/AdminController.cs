using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NotesApp.Data;

namespace NotesApp.Controllers;

// [OWASP A01 - eskalacja pionowa] tylko [Authorize] (zalogowanie), BRAK [Authorize(Roles="Admin")] - celowo.
[Authorize]
public class AdminController : Controller
{
    private readonly AppDbContext _db;
    public AdminController(AppDbContext db) { _db = db; }

    [HttpGet("/admin")]
    public async Task<IActionResult> Index()
        => View(await _db.Notes.Include(n => n.Owner).ToListAsync());
}
