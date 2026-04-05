
using TechSpherex.CleanArchitecture.Domain.Entities;
using Microsoft.EntityFrameworkCore;
namespace TechSpherex.CleanArchitecture.Application.Abstractions.Data;

public interface IAppDbContext
{
    DbSet<TodoItem> Todos { get; }
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
