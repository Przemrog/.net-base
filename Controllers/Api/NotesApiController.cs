using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NotesApp.Data;

namespace NotesApp.Controllers.Api;

[ApiController]
[Route("api/notes")]
[Authorize(AuthenticationSchemes = "Bearer")]
public class NotesApiController : ControllerBase
{
    private readonly AppDbContext _db;
    public NotesApiController(AppDbContext db) { _db = db; }

    private string CurrentUserId => User.FindFirstValue(ClaimTypes.NameIdentifier)!;

    [HttpGet]
    public async Task<IActionResult> Mine()
        => Ok(await _db.Notes.Where(n => n.OwnerId == CurrentUserId).ToListAsync());

    [HttpGet("{id:int}")]
    public async Task<IActionResult> One(int id)
    {
        // [OWASP A01 - IDOR] brak weryfikacji wlasciciela - celowo w wariancie bazowym
        var note = await _db.Notes.FindAsync(id);
        return note is null ? NotFound() : Ok(note);
    }
}
