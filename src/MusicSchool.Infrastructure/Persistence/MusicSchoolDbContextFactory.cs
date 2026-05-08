using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace MusicSchool.Infrastructure.Persistence;

public sealed class MusicSchoolDbContextFactory : IDesignTimeDbContextFactory<MusicSchoolDbContext>
{
    public MusicSchoolDbContext CreateDbContext(string[] args)
    {
        var connectionString = Environment.GetEnvironmentVariable("MUSICSCHOOL_CONNECTION_STRING")
            ?? "Server=(localdb)\\mssqllocaldb;Database=MusicSchool;Trusted_Connection=True;MultipleActiveResultSets=true;TrustServerCertificate=True";

        var options = new DbContextOptionsBuilder<MusicSchoolDbContext>()
            .UseSqlServer(connectionString)
            .Options;

        return new MusicSchoolDbContext(options);
    }
}
