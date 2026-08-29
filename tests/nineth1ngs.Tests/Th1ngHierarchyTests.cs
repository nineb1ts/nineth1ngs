using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using nineth1ngs.Data;
using nineth1ngs.Models;

namespace nineth1ngs.Tests;

public sealed class Th1ngHierarchyTests
{
    [Fact]
    public void ParentRelation_UsesCascadeDelete()
    {
        using var connection = new SqliteConnection("Data Source=:memory:");
        connection.Open();
        var options = new DbContextOptionsBuilder<Th1ngDbContext>()
            .UseSqlite(connection)
            .Options;

        using (var database = new Th1ngDbContext(options))
        {
            database.Database.Migrate();
            var parent = new Th1ng { Text = "Parent", CreatedAt = DateTime.UtcNow };
            database.Th1ngs.Add(parent);
            database.SaveChanges();

            database.Th1ngs.Add(new Th1ng
            {
                Text = "Child",
                CreatedAt = DateTime.UtcNow,
                ParentId = parent.Id
            });
            database.SaveChanges();

            database.Th1ngs.Remove(parent);
            database.SaveChanges();
        }

        using var verification = new Th1ngDbContext(options);
        Assert.Empty(verification.Th1ngs);
    }

    [Fact]
    public void SubTh1ng_HasParentIdAndNoTimerByDefault()
    {
        var subTh1ng = new Th1ng
        {
            ParentId = 42,
            Text = "Child"
        };

        Assert.True(subTh1ng.IsSubTh1ng);
        Assert.False(subTh1ng.IsTimerRunning);
        Assert.Equal(0, subTh1ng.ElapsedSeconds);
        Assert.Null(subTh1ng.TimerStartedAt);
    }
}
