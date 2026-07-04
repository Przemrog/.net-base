using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace NotesApp.Controllers;

[Authorize]
public class DebugController : Controller
{
    [HttpGet("/debug/error")]
    public IActionResult Error(string? input)
    {
        // [OWASP A10] celowe wejscie w stan wyjatkowy; brak globalnej obslugi -> wyciek Stack Trace do klienta
        var value = int.Parse(input!);
        return Content($"Parsed: {value}");
    }
}
