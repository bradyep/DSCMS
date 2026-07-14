# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## What this is

DSCMS ("Dead Simple CMS") is an ASP.NET Core 10 / EF Core (SQLite) content management system. It serves public content on a dynamic `/{contentTypeName}/{contentUrl?}` route and an admin UI under `/Admin` for managing content, layouts, templates, and users. It's deployed as a Docker container (see README.md for the docker build/push/deploy steps used for the production host).

## Commands

Build / run:
```powershell
dotnet build DSCMS/DSCMS.csproj
dotnet run --project DSCMS/DSCMS.csproj
```

Unit tests (controllers, fast, no running app needed):
```powershell
dotnet test DSCMS.Tests --filter "FullyQualifiedName~Controllers"
```

E2E tests (Playwright, drives a real Chromium browser against a real app instance on port 5099):
```powershell
# one-time browser install per machine (or after a Playwright version bump)
pwsh DSCMS.Tests\bin\Debug\net10.0\playwright.ps1 install chromium

# convenience script — starts/stops the app for you
pwsh .\Run-E2ETests.ps1

# or directly; PlaywrightFixture starts/stops the app itself
dotnet test DSCMS.Tests --filter "FullyQualifiedName~E2E"

# watch it run in a visible browser window
$env:HEADED = "1"; dotnet test DSCMS.Tests --filter "FullyQualifiedName~E2E"
```

Run a single test: add `--filter "FullyQualifiedName~ClassName.MethodName"` (standard xUnit/dotnet test filtering).

EF Core migrations (run from repo root; project + startup-project both point at `DSCMS/DSCMS.csproj`):
```powershell
dotnet ef migrations add <Name> --project DSCMS --startup-project DSCMS
dotnet ef database update --project DSCMS --startup-project DSCMS
```

## Architecture

### Content model

The CMS is built from six core EF entities (`DSCMS/Models/`) wired together in `DSCMS/Data/ApplicationDbContext.cs`:

- **ContentType** — a category of content (e.g. "blog", "games"). Has a `Name` (used in URLs), an `ItemsPerPage` for pagination, and points at two templates: `MultipleContentsTemplate` (list view) and `DefaultSingleContentTemplate` (fallback single-item view).
- **Content** — one item of a ContentType (e.g. a blog post). Has `UrlToDisplay` (slug), `BodySource`, and an optional `TemplateId` that overrides the ContentType's default.
- **ContentTypeField** — defines a named custom field on a ContentType (e.g. "teaser").
- **ContentTypeFieldItem** — the actual value of a ContentTypeField for one Content instance.
- **Template** — a Razor view path (or inline source) plus a `SourceType` and an optional `Layout`. Can be flagged `IsForMultipleContents`.
- **Layout** — the outer HTML shell a Template renders into.
- **SourceType** — enum-like lookup (RazorFile / InlineRazor / Markdown / HTML / Text) seeded via `HasData`, shared by Template, Layout, and Content.BodySourceType.

Note: the `ContentTypeField`/`ContentTypeFieldItem` tables were originally named `ContentTypeItem`/`ContentItem` — renamed via the `RenameContentTypeItemsToContentTypeFields` migration. `DiagnosticsController.cs` still references the old names in some backup-restore/repair code paths that operate on pre-migration data; that's expected, not a bug.

### Public content rendering

All public traffic funnels through `DSCMSController.Content` (`DSCMS/Controllers/DSCMSController.cs`), mapped by the `cms` and `default` routes in `Program.cs` as `{contentTypeName}/{contentUrl?}`. Given `contentTypeName` it looks up the `ContentType`; if `contentUrl` is also present it resolves a specific `Content` row, otherwise it lists/paginates all Content for that type. It then picks a Template (falling back through Content → ContentType default → a `Bootstrap{ContentTypeName}` convention view → an empty template) and renders `content.Template.Layout` around it. If no `ContentType` matches at all, it shows a first-run welcome page — this is how a fresh install behaves before any admin setup.

### Admin layer: API + view-shell split

Each admin resource (Contents, ContentTypes, ContentTypeFields, ContentTypeFieldItems, Layouts, Templates, Users) has **two controllers**:
- `{Resource}Controller` — `[Authorize] [ApiController] [Route("api/[controller]")]`, does all real CRUD against the repository layer and returns DTOs (`DSCMS/Models/DTOs/`).
- `{Resource}ViewController` — `[Authorize]` MVC controller that only returns the Razor shell pages under `Views/{Resource}/*.cshtml`; it holds no business logic.

The shell pages are then driven client-side by matching JS under `wwwroot/js/{resource}/{index,create,edit,details,delete}.js`, which call the `api/[controller]` endpoints. When adding an admin feature, you generally touch four things per action: the API controller method + DTO, the Razor shell view, and its JS file — plus the corresponding route registration in `Program.cs` (routes are explicit per admin resource, not a single catch-all).

This split is a holdover from DSCMS's original incarnation as a traditional MVC app (~9 years old), not a deliberate design choice — the plan is to eventually replace the ViewController + JS shell layer with Blazor, while the API controllers stick around as the data layer underneath. Don't "clean up" the split as an inconsistency; if asked to build new admin functionality, it's worth confirming whether it should follow the existing Razor+JS pattern or start using Blazor instead.

### Data access

All EF access goes through a repository per entity (`DSCMS/Repositories/Interfaces` + `Implementations`), registered as scoped services in `Program.cs`. Controllers depend on repository interfaces, never on `ApplicationDbContext` directly. Repository methods are named for their `Include`/filter shape (e.g. `GetByContentTypeIdWithDetailsAsync`, `GetByIdWithFieldItemsAsync`) rather than being generic — follow that convention (specific, intention-revealing method names) rather than adding generic `Get`/`Query` overloads.

### Identity

Uses ASP.NET Core Identity (`ApplicationUser : IdentityUser`) with EF store via `ApplicationDbContext : IdentityDbContext<ApplicationUser>`. Password policy is deliberately relaxed (no digit/uppercase/special-char requirements, min length 6) — this is intentional for a personal-site CMS, not an oversight.

### Testing

- Controller unit tests (`DSCMS.Tests/Controllers/`) use xUnit + Moq, mocking repositories — no database or running app required.
- E2E tests (`DSCMS.Tests/E2E/`) use Playwright + `PlaywrightFixture`, which starts a real `dotnet run` instance on a dedicated port (5099, separate from the normal dev port 5000) via `IAsyncLifetime`, and tears it down after the test collection. They exercise real rendering (blog listing/pagination, Games/Projects/About sections) rather than mocking anything.

### Configuration

`appsettings.json` is for local dev; `appsettings.Production.json` is copied in during the Docker build and used for production. `IConfigurationService` (`DSCMS/Services/`) wraps typed access to config sections (EmailSettings, DatabaseSettings) rather than controllers reading `IConfiguration` directly.
