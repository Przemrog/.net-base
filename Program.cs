using System.Text;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using NotesApp.Data;
using NotesApp.Models;

var builder = WebApplication.CreateBuilder(args);

var conn = builder.Configuration.GetConnectionString("Default");
builder.Services.AddDbContext<AppDbContext>(o => o.UseNpgsql(conn));

// ASP.NET Core Identity = wbudowana "bateria" .NET: haszowanie PBKDF2, zarzadzanie tozsamoscia.
// W wariancie BAZOWYM pozostawiamy lagodna, domyslna polityke i NIE wlaczamy blokady konta.
builder.Services.AddIdentity<ApplicationUser, IdentityRole>(options =>
    {
        options.Password.RequiredLength = 6;
        options.Password.RequireNonAlphanumeric = false;
        options.Password.RequireUppercase = false;
        options.Password.RequireDigit = false;
        // [OWASP A07] brak lockout (SignIn.RequireConfirmedAccount, Lockout) - celowo w wariancie bazowym
    })
    .AddEntityFrameworkStores<AppDbContext>()
    .AddDefaultTokenProviders();

// JWT jako dodatkowy schemat uwierzytelniania dla warstwy /api
var jwtKey = builder.Configuration["Jwt:Key"] ?? "insecure-dev-key";
builder.Services.AddAuthentication()
    .AddJwtBearer("Bearer", options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            // [OWASP A08] brak walidacji wydawcy i odbiorcy tokenu - celowo w wariancie bazowym
            ValidateIssuer = false,
            ValidateAudience = false,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey))
        };
    });

builder.Services.AddControllersWithViews();
// [OWASP A01/SSRF] wspoldzielony HttpClient uzywany przez funkcje importu z URL
builder.Services.AddHttpClient();

var app = builder.Build();

// [OWASP A02 / A10] szczegolowa strona bledu (Stack Trace) wlaczona zawsze - celowo w wariancie bazowym.
// Brak globalnej obslugi wyjatkow -> nieobsluzone wyjatki wyciekaja do klienta.
app.UseDeveloperExceptionPage();

// [OWASP A02] BRAK middleware naglowkow bezpieczenstwa (CSP/HSTS/X-Frame-Options) - celowo.

app.UseStaticFiles();
app.UseRouting();
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

// Inicjalizacja bazy i danych testowych (min. dwoch uzytkownikow => testy IDOR).
using (var scope = app.Services.CreateScope())
{
    await DbSeeder.SeedAsync(scope.ServiceProvider);
}

app.Run();
