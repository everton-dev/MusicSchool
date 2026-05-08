using System.Globalization;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.AspNetCore.Localization;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.OpenApi;
using MusicSchool.API.Auth;
using MusicSchool.Infrastructure;
using MusicSchool.Infrastructure.Persistence;

var builder = WebApplication.CreateBuilder(args);

var supportedCultures = new[]
{
    new CultureInfo("en-US"),
    new CultureInfo("en-GB"),
    new CultureInfo("pt-PT"),
    new CultureInfo("pt-BR"),
    new CultureInfo("es-ES")
};

builder.Services.AddControllers();
builder.Services.AddLocalization(options => options.ResourcesPath = "Resources");
builder.Services.Configure<RequestLocalizationOptions>(options =>
{
    options.DefaultRequestCulture = new RequestCulture("en-US");
    options.SupportedCultures = supportedCultures;
    options.SupportedUICultures = supportedCultures;
});
builder.Services.AddInfrastructure(builder.Configuration);
builder.Services.AddApiAuth(builder.Configuration);
builder.Services.AddHealthChecks()
    .AddCheck("self", () => HealthCheckResult.Healthy(), tags: ["live"])
    .AddDbContextCheck<MusicSchoolDbContext>("database", tags: ["ready"]);
builder.Services.AddOpenApi(options =>
{
    options.AddDocumentTransformer((document, _, _) =>
    {
        const string bearerSchemeName = "Bearer";

        document.Components ??= new OpenApiComponents();
        document.Components.SecuritySchemes ??= new Dictionary<string, IOpenApiSecurityScheme>(StringComparer.Ordinal);
        document.Components.SecuritySchemes[bearerSchemeName] = new OpenApiSecurityScheme
        {
            Type = SecuritySchemeType.Http,
            Scheme = "bearer",
            BearerFormat = "JWT",
            Description = "JWT bearer token authentication."
        };

        document.Security ??= [];
        document.Security.Add(new OpenApiSecurityRequirement
        {
            [new OpenApiSecuritySchemeReference(bearerSchemeName, document, externalResource: null)] = []
        });

        return Task.CompletedTask;
    });
});

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();
app.UseRequestLocalization();
app.UseAuthentication();
app.UseMiddleware<TenantResolutionMiddleware>();

app.UseAuthorization();

app.MapHealthChecks("/health/live", new HealthCheckOptions
{
    Predicate = check => check.Tags.Contains("live")
});
app.MapHealthChecks("/health/ready", new HealthCheckOptions
{
    Predicate = check => check.Tags.Contains("ready")
});
app.MapControllers();

app.Run();

public partial class Program;
