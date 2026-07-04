using Microsoft.AspNetCore.Mvc;

namespace NotesApp.Controllers;

public class HomeController : Controller
{
    [HttpGet("/")] public IActionResult Index() => Redirect("/notes");
}
