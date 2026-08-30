using Microsoft.EntityFrameworkCore;
using nineth1ngs.Data;
using nineth1ngs.Models;

namespace nineth1ngs.Services;

public sealed class Th1ngStore
{
    public async Task<IReadOnlyList<Th1ng>> LoadAsync(CancellationToken cancellationToken = default)
    {
        await using var database = new Th1ngDbContext();

        return await database.Th1ngs
            .AsNoTracking()
            .OrderByDescending(th1ng => th1ng.CreatedAt)
            .ToListAsync(cancellationToken);
    }

    public async Task AddAsync(Th1ng th1ng, CancellationToken cancellationToken = default)
    {
        await using var database = new Th1ngDbContext();

        database.Th1ngs.Add(th1ng);
        await database.SaveChangesAsync(cancellationToken);
    }

    public async Task UpdateAsync(Th1ng th1ng, CancellationToken cancellationToken = default)
    {
        await using var database = new Th1ngDbContext();

        database.Th1ngs.Update(th1ng);
        await database.SaveChangesAsync(cancellationToken);
    }

    public async Task DeleteAsync(Th1ng th1ng, CancellationToken cancellationToken = default)
    {
        await using var database = new Th1ngDbContext();

        database.Th1ngs.Remove(th1ng);
        await database.SaveChangesAsync(cancellationToken);
    }
}
