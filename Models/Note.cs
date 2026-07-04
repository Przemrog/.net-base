using System.ComponentModel.DataAnnotations;

namespace NotesApp.Models;

public class Note
{
    public int Id { get; set; }

    // Relacja wlasciciela - podstawa testow poziomej kontroli dostepu (IDOR).
    public string OwnerId { get; set; } = string.Empty;
    public ApplicationUser? Owner { get; set; }

    [Required]
    public string Title { get; set; } = string.Empty;

    public string Body { get; set; } = string.Empty;

    public bool IsPublic { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
}
