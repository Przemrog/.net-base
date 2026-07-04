# Aplikacja referencyjna — .NET (ASP.NET Core), WARIANT BAZOWY

Implementacja aplikacji-notatnika zgodna ze specyfikacją z rozdziału 6 pracy.
To jest **wariant domyślny (bazowy)**: aplikacja napisana idiomatycznie dla ASP.NET Core,
bez świadomego utwardzania. Realistyczne słabości domyślne oraz celowe „naiwne" wzorce
oznaczono w kodzie komentarzem `[OWASP Axx]`, aby można było się na nie wprost powołać
w rozdziale wynikowym i porównać z wariantem utwardzonym.

## Stos
- .NET 8 (ASP.NET Core MVC), Razor Views
- Entity Framework Core + Npgsql (PostgreSQL 16)
- ASP.NET Core Identity (haszowanie haseł PBKDF2 — wbudowana „bateria" .NET)
- JWT Bearer dla warstwy `/api`
- Docker Compose (aplikacja + baza w osobnych kontenerach)

## Uruchomienie
```bash
docker compose up --build
```
Aplikacja: http://localhost:8080  (przekierowuje na /notes)

Baza inicjalizuje się sama (EnsureCreated) i zakłada konta testowe:

| Konto          | Hasło    | Rola  |
|----------------|----------|-------|
| admin@local    | admin123 | Admin |
| alice@local    | alice123 | User  |
| bob@local      | bob123   | User  |

Dwa konta User z własnymi notatkami umożliwiają testy IDOR (Alicja próbuje odczytać notatkę Boba).

## Mapa powierzchni ataku (do rozdziału 7)

| Kategoria OWASP 2025 | Gdzie w kodzie | Charakter w wariancie bazowym |
|---|---|---|
| A01 IDOR (poziomo)   | `NotesController.Details/Edit/Delete`, `NotesApiController.One` | brak weryfikacji właściciela |
| A01 eskalacja (pionowo) | `AdminController` | `[Authorize]` bez roli — każdy zalogowany wchodzi na /admin |
| A01 SSRF             | `NotesController.Import` | pobieranie dowolnego URL bez walidacji |
| A02 Misconfiguration | `Program.cs` | brak nagłówków bezpieczeństwa; Development => Stack Trace |
| A03 Supply Chain     | `NotesApp.csproj` | miejsce na pakiet z CVE (do potwierdzenia skanerem) |
| A04 Cryptographic    | Identity (PBKDF2) | hasła haszowane domyślnie — oczekiwany wynik: chronione |
| A05 SQL Injection    | `NotesController.Search` | surowy SQL z interpolacją (`FromSqlRaw`) |
| A05 XSS (stored)     | `Views/Notes/Details.cshtml` | `@Html.Raw(Model.Body)` |
| A06 Insecure Design  | brak rate limiting, brak ograniczeń importu | ocena jakościowa |
| A07 Auth Failures    | `AccountController.Login` | brak lockout/rate limiting; słaba polityka haseł |
| A08 Integrity (JWT)  | `Program.cs`, `AuthApiController` | brak walidacji issuer/audience; token 24 h |
| A09 Logging          | `AccountController.Login` | nieudane logowania nie są audytowane |
| A10 Exceptional      | `DebugController.Error` | nieobsłużony wyjątek => wyciek Stack Trace |

## Szybka weryfikacja (przykłady)
- IDOR: zaloguj jako alice, otwórz `/notes/3` (notatka Boba) — w wariancie bazowym się wyświetli.
- Eskalacja: jako alice wejdź na `/admin` — w wariancie bazowym dostęp jest.
- XSS: utwórz notatkę z treścią `<script>alert(1)</script>` i otwórz jej podgląd.
- SQLi: `/notes/search?q=%' OR '1'='1` — zwróci notatki spoza filtra tytułu.
- A10: `/debug/error?input=abc` — zwróci stronę wyjątku ze śladem stosu.

## Uwaga
Aplikacja jest celowo pozbawiona utwardzenia i służy **wyłącznie** do kontrolowanych testów
w izolowanym środowisku na potrzeby pracy magisterskiej. Nie należy jej wystawiać do sieci.
