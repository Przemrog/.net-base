using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using NotesApp.Models;

namespace NotesApp.Data;

public static class DbSeeder
{
    public static async Task SeedAsync(IServiceProvider sp)
    {
        var db = sp.GetRequiredService<AppDbContext>();
        await db.Database.EnsureCreatedAsync();

        var roleMgr = sp.GetRequiredService<RoleManager<IdentityRole>>();
        var userMgr = sp.GetRequiredService<UserManager<ApplicationUser>>();

        foreach (var role in new[] { "Admin", "User" })
            if (!await roleMgr.RoleExistsAsync(role))
                await roleMgr.CreateAsync(new IdentityRole(role));

        var admin = await EnsureUser(userMgr, "admin@local", "admin123", "Admin");
        var alice = await EnsureUser(userMgr, "alice@local", "alice123", "User");
        var bob   = await EnsureUser(userMgr, "bob@local",   "bob123",   "User");

        if (!await db.Notes.AnyAsync())
        {
            db.Notes.AddRange(
                new Note { OwnerId = alice.Id, Title = "Lista zakupow", Body = "mleko, chleb, kawa" },
                new Note { OwnerId = alice.Id, Title = "Haslo do routera", Body = "prywatna notatka Alicji" },
                new Note { OwnerId = bob.Id,   Title = "Pomysly na projekt", Body = "prywatna notatka Boba" },
                new Note { OwnerId = admin.Id, Title = "Notatka administratora", Body = "tylko dla admina" }
            );
            await db.SaveChangesAsync();
        }
    }

    private static async Task<ApplicationUser> EnsureUser(
        UserManager<ApplicationUser> mgr, string email, string password, string role)
    {
        var user = await mgr.FindByEmailAsync(email);
        if (user is null)
        {
            user = new ApplicationUser { UserName = email, Email = email, EmailConfirmed = true };
            await mgr.CreateAsync(user, password);
            await mgr.AddToRoleAsync(user, role);
        }
        return user;
    }
}
