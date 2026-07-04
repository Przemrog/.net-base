using Microsoft.AspNetCore.Identity;

namespace NotesApp.Models;

public class ApplicationUser : IdentityUser
{
    public string? AvatarPath { get; set; }
}
