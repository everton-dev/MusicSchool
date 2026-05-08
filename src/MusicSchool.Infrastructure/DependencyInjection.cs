using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using MusicSchool.Application.Abstractions;
using MusicSchool.Application.Curriculum;
using MusicSchool.Application.Families;
using MusicSchool.Application.Lessons;
using MusicSchool.Application.Payments;
using MusicSchool.Domain.Repositories;
using MusicSchool.Infrastructure.Persistence;

namespace MusicSchool.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("MusicSchool")
            ?? "Server=(localdb)\\mssqllocaldb;Database=MusicSchool;Trusted_Connection=True;MultipleActiveResultSets=true;TrustServerCertificate=True";

        services.AddDbContext<MusicSchoolDbContext>(options => options.UseSqlServer(connectionString));

        services.Configure<BlobStorageOptions>(options =>
        {
            var section = configuration.GetSection("BlobStorage");
            options.ConnectionString = section["ConnectionString"] ?? options.ConnectionString;
            options.ContainerName = section["ContainerName"] ?? options.ContainerName;
        });
        services.Configure<EmailOptions>(options =>
        {
            var section = configuration.GetSection("Email");
            options.Host = section["Host"] ?? options.Host;
            options.Port = int.TryParse(section["Port"], out var port) ? port : options.Port;
            options.From = section["From"] ?? options.From;
            options.UserName = section["UserName"];
            options.Password = section["Password"];
            options.EnableSsl = bool.TryParse(section["EnableSsl"], out var enableSsl) && enableSsl;
        });

        services.AddSingleton<IClock, SystemClock>();
        services.AddScoped<IBlobStorageService, AzureBlobStorageService>();
        services.AddScoped<IEmailSender, SmtpEmailSender>();
        services.AddScoped<IFamilyGroupRepository, FamilyGroupRepository>();
        services.AddScoped<ILessonRepository, LessonRepository>();
        services.AddScoped<ITeacherRepository, TeacherRepository>();
        services.AddScoped<IStudentRepository, StudentRepository>();
        services.AddScoped<ICurriculumRepository, CurriculumRepository>();
        services.AddScoped<IStudentCurriculumProgressRepository, StudentCurriculumProgressRepository>();
        services.AddScoped<IPaymentRepository, PaymentRepository>();
        services.AddScoped<IUserRepository, UserRepository>();
        services.AddScoped<IUnitOfWork, EfUnitOfWork>();
        services.AddScoped<ICurriculumService, CurriculumService>();
        services.AddScoped<IFamilyGroupService, FamilyGroupService>();
        services.AddScoped<ILessonSchedulingService, LessonSchedulingService>();
        services.AddScoped<IPaymentService, PaymentService>();

        return services;
    }
}
