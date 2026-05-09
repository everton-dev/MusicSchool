using Microsoft.AspNetCore.Authentication.JwtBearer;
using MusicSchool.Application.Abstractions;
using MusicSchool.Domain.Users;

namespace MusicSchool.API.Auth;

public static class AuthServiceCollectionExtensions
{
    public static IServiceCollection AddApiAuth(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddScoped<CurrentTenant>();
        services.AddScoped<ITenantContext>(provider => provider.GetRequiredService<CurrentTenant>());

        services
            .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
            .AddJwtBearer(options =>
            {
                options.Authority = configuration["Authentication:Authority"];
                options.Audience = configuration["Authentication:Audience"];
                options.RequireHttpsMetadata = !string.Equals(configuration["Authentication:RequireHttpsMetadata"], "false", StringComparison.OrdinalIgnoreCase);
                options.MapInboundClaims = false;
            });

        services.AddAuthorization(options =>
        {
            options.AddPolicy(AuthConstants.Policies.AdminOnly, policy => policy.RequireRole(UserRole.Admin.ToString()));
            options.AddPolicy(AuthConstants.Policies.AdminOrTeacher, policy => policy.RequireRole(UserRole.Admin.ToString(), UserRole.Teacher.ToString()));
            options.AddPolicy(AuthConstants.Policies.AdminOrGuardian, policy => policy.RequireRole(UserRole.Admin.ToString(), UserRole.Guardian.ToString()));
            options.AddPolicy(AuthConstants.Policies.AdminTeacherOrStudent, policy => policy.RequireRole(
                UserRole.Admin.ToString(),
                UserRole.Teacher.ToString(),
                UserRole.Student.ToString()));
            options.AddPolicy(AuthConstants.Policies.AdminTeacherGuardianOrStudent, policy => policy.RequireRole(
                UserRole.Admin.ToString(),
                UserRole.Teacher.ToString(),
                UserRole.Guardian.ToString(),
                UserRole.Student.ToString()));
        });

        return services;
    }
}
