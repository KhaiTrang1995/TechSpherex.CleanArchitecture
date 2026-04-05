
using TechSpherex.CleanArchitecture.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace TechSpherex.CleanArchitecture.Application.UnitTests;
public static class TestDbContextFactory
{
    public static AppDbContext Create()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        return new AppDbContext(options);
    }
}
