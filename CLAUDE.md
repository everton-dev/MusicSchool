# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Commands

### Backend (.NET 10)
```bash
# Run the API
dotnet run --project src/MusicSchool.API

# Run all tests
dotnet test

# Run only unit tests
dotnet test tests/MusicSchool.UnitTests

# Run only integration tests
dotnet test tests/MusicSchool.IntegrationTests

# Run a specific test class or method
dotnet test --filter "FullyQualifiedName~FamilyGroupServiceTests"

# Add an EF Core migration
dotnet ef migrations add <MigrationName> --project src/MusicSchool.Infrastructure --startup-project src/MusicSchool.API

# Apply pending migrations
dotnet ef database update --project src/MusicSchool.Infrastructure --startup-project src/MusicSchool.API
```

### Frontend (Angular 21)
```bash
cd src/MusicSchool.Web

# Dev server (http://localhost:4200)
npm start

# Production build
npm run build

# Extract i18n messages
npm run extract-i18n
```

## Architecture

### Backend — Clean Architecture (4 layers)

```
src/MusicSchool.Domain/        # Entities, value objects, repository interfaces, domain errors
src/MusicSchool.Application/   # Services, commands/queries, DTOs, mappers, service abstractions
src/MusicSchool.Infrastructure/ # EF Core, migrations, repository impls, blob storage, SMTP
src/MusicSchool.API/           # Controllers, request/response contracts, auth, middleware
```

**Dependency direction**: API → Application → Domain ← Infrastructure

**Key patterns**:
- All IDs are strongly-typed `readonly record struct` wrappers over `Guid` (e.g. `UserId`, `TenantId`, `LessonId`) — defined in `Domain/Common/Ids.cs`.
- `Result<T>` / `Result` pattern used for domain operation outcomes — never throw for business rule violations.
- Repository interfaces live in `Domain/Repositories/`; EF implementations live in `Infrastructure/Persistence/`.
- `IUnitOfWork` (EF-backed) is used to commit changes; repositories themselves don't call `SaveChanges`.
- Application services take repository/UoW interfaces — no direct EF dependency in Application.

**Multi-tenancy**: Every entity has a `TenantId`. The API resolves it in `TenantResolutionMiddleware` from either the `X-Tenant-Id` request header or the `tenant_id` JWT claim. `ITenantContext` (implemented by `CurrentTenant`) is injected into services to scope queries.

**Auth**: JWT Bearer (`Authentication:Authority` / `Authentication:Audience` in appsettings). Authorization policies: `AdminOnly`, `AdminOrTeacher`, `AdminOrGuardian`, `AdminTeacherOrStudent`, `AdminTeacherGuardianOrStudent`. Roles: `Admin`, `Teacher`, `Guardian`, `Student`.

**Localization**: Supported cultures — en-US (default), en-GB, pt-PT, pt-BR, es-ES. String resources in `src/MusicSchool.API/Resources/`.

**External services** (dev defaults):
- SQL Server: `(localdb)\mssqllocaldb` database `MusicSchool`
- Blob storage: Azurite (`UseDevelopmentStorage=true`)
- Email: localhost SMTP on port 25

**Health endpoints**: `/health/live` (liveness), `/health/ready` (DB check).  
**OpenAPI**: `/openapi/v1.json` (development only).

### Frontend — Angular 21 SPA

Single service entry point: `MusicSchoolApiService` (`src/app/core/api/music-school-api.service.ts`) — all HTTP calls go through this service.

`tenantInterceptor` automatically attaches `X-Tenant-Id` header on every request, reading from `localStorage['music-school-tenant-id']` (falls back to a hardcoded default GUID).

Modules use lazy-loaded routes. Feature modules: `dashboard`, `lessons`, `families`, `users`, `teachers`, `curriculum`, `payments`. Each module has a `*.routes.ts` file and a top-level feature component.

Auth is handled by `AuthService` + `authGuard` / `loginRedirectGuard` in `core/guards/auth.guard.ts`.

### Tests

**Unit tests** (`MusicSchool.UnitTests`): xUnit + Moq + FluentAssertions. Test application services against mocked repositories and `IUnitOfWork`. A `TestTenantContext` helper sets up the tenant for tests.

**Integration tests** (`MusicSchool.IntegrationTests`): `WebApplicationFactory<Program>` with EF InMemory database replacing SQL Server. `FakeBlobStorageService` and `FakeEmailSender` replace real external services. `TestAuthExtensions.CreateAuthenticatedClient(UserRole, tenantId)` bootstraps authenticated HTTP clients.
