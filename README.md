# MusicSchool

Modular monolith scaffold for a music school management system.

## Structure

- `src/MusicSchool.Domain`: DDD entities, value objects, result primitives, and repository interfaces.
- `src/MusicSchool.Application`: commands, DTOs, and application service abstractions.
- `src/MusicSchool.Infrastructure`: EF Core SQL Server persistence, Azure Blob Storage, SMTP email, and DI wiring.
- `src/MusicSchool.API`: ASP.NET Core API, localization setup, and controllers.
- `tests/MusicSchool.UnitTests`: domain and application tests with xUnit, FluentAssertions, and Moq.
- `tests/MusicSchool.IntegrationTests`: API integration tests with `WebApplicationFactory`.
- `sql`: Azure SQL-compatible schema scripts.

All server timestamps are represented as UTC. Weekly lessons keep local lesson time plus time zone id because fixed weekly teaching slots are calendar rules, not instants.

## Backend Operations

Restore local tools before working with EF Core migrations:

```powershell
dotnet tool restore
```

The EF design-time factory reads `MUSICSCHOOL_CONNECTION_STRING` and falls back to LocalDB for local development. For Azure SQL, set the environment variable before running migrations:

```powershell
$env:MUSICSCHOOL_CONNECTION_STRING = "Server=tcp:<server>.database.windows.net,1433;Initial Catalog=<database>;Encrypt=True;TrustServerCertificate=False;Connection Timeout=30;Authentication=Active Directory Default"
dotnet tool run dotnet-ef database update --project src\MusicSchool.Infrastructure\MusicSchool.Infrastructure.csproj --startup-project src\MusicSchool.API\MusicSchool.API.csproj
```

The API exposes:

- `/api/health`: localized application health response.
- `/health/live`: process liveness probe.
- `/health/ready`: database readiness probe.
- `/openapi/v1.json`: OpenAPI document with JWT bearer security metadata in development.

The API project enables trim and Native AOT compatibility analysis. It intentionally does not enable `PublishAot` by default because ASP.NET Core MVC controllers are not fully trim/native-AOT compatible; the current setting surfaces analyzer warnings while keeping the controller-based scaffold usable. A future Native AOT track should migrate the API surface to source-generated/minimal endpoints before enabling `PublishAot=true` as a release default.

Use a self-contained publish when smoke testing trimming:

```powershell
dotnet publish src\MusicSchool.API\MusicSchool.API.csproj -c Release -r win-x64 -p:PublishTrimmed=true --self-contained true
```

Trim warnings are expected in the current controller/EF Core scaffold and should be treated as the backlog for the Native AOT hardening stage.

## Frontend

Run the Angular app from `src/MusicSchool.Web`:

```powershell
npm install
npm start
```

The frontend is an Angular Material operations workspace with runtime translations for `en-US`, `pt-PT`, `pt-BR`, and `es-ES`. A four-flag toolbar selector updates the visible shell immediately, and the typed API client sends `X-Tenant-Id` through an HTTP interceptor.
