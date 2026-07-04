using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NotesApp.Data;
using NotesApp.Models;

namespace NotesApp.Controllers;

[Authorize]
public class NotesController : Controller
{
    private readonly AppDbContext _db;
    private readonly UserManager<ApplicationUser> _users;
    private readonly IHttpClientFactory _http;

    public NotesController(AppDbContext db, UserManager<ApplicationUser> users, IHttpClientFactory http)
    { _db = db; _users = users; _http = http; }

    private string CurrentUserId => _users.GetUserId(User)!;

    [HttpGet("/notes")]
    public async Task<IActionResult> Index()
        => View(await _db.Notes.Where(n => n.OwnerId == CurrentUserId).ToListAsync());

    [HttpGet("/notes/{id:int}")]
    public async Task<IActionResult> Details(int id)
    {
        // [OWASP A01 - IDOR] pobranie po samym id, BEZ weryfikacji wlasciciela - celowo w wariancie bazowym
        var note = await _db.Notes.FindAsync(id);
        return note is null ? NotFound() : View(note);
    }

    [HttpGet("/notes/new")] public IActionResult Create() => View();

    [HttpPost("/notes")]
    public async Task<IActionResult> Create(string title, string body)
    {
        // Tresc zapisywana bez sanityzacji; ekspozycja XSS zalezy od renderowania w widoku Details.
        _db.Notes.Add(new Note { OwnerId = CurrentUserId, Title = title, Body = body });
        await _db.SaveChangesAsync();
        return Redirect("/notes");
    }

    [HttpGet("/notes/{id:int}/edit")]
    public async Task<IActionResult> Edit(int id)
    {
        var note = await _db.Notes.FindAsync(id); // [OWASP A01] brak weryfikacji wlasciciela
        return note is null ? NotFound() : View(note);
    }

    [HttpPost("/notes/{id:int}/edit")]
    public async Task<IActionResult> Edit(int id, string title, string body)
    {
        var note = await _db.Notes.FindAsync(id); // [OWASP A01] brak weryfikacji wlasciciela
        if (note is null) return NotFound();
        note.Title = title; note.Body = body; note.UpdatedAt = DateTime.UtcNow;
        await _db.SaveChangesAsync();
        return Redirect($"/notes/{id}");
    }

    [HttpPost("/notes/{id:int}/delete")]
    public async Task<IActionResult> Delete(int id)
    {
        var note = await _db.Notes.FindAsync(id); // [OWASP A01] brak weryfikacji wlasciciela
        if (note is not null) { _db.Notes.Remove(note); await _db.SaveChangesAsync(); }
        return Redirect("/notes");
    }

    [HttpGet("/notes/search")]
    public async Task<IActionResult> Search(string q)
    {
        // [OWASP A05 - SQL Injection] naiwna interpolacja do surowego SQL - celowo w wariancie bazowym.
        // Idiomatyczne LINQ byloby bezpieczne; tu odwzorowujemy typowy blad poczatkujacego programisty.
        var sql = $"SELECT * FROM \"Notes\" WHERE \"OwnerId\" = '{CurrentUserId}' AND \"Title\" ILIKE '%{q}%'";
        var notes = await _db.Notes.FromSqlRaw(sql).ToListAsync();
        ViewBag.Query = q;
        return View("Index", notes);
    }

    [HttpPost("/notes/import")]
    public async Task<IActionResult> Import(string url)
    {
        // [OWASP A01 - SSRF] serwer pobiera dowolny URL bez walidacji - celowo w wariancie bazowym
        var content = await _http.CreateClient().GetStringAsync(url);
        _db.Notes.Add(new Note { OwnerId = CurrentUserId, Title = $"Import z {url}", Body = content });
        await _db.SaveChangesAsync();
        return Redirect("/notes");
    }
}
