using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace GymRoutineGenerator.Data.Context;

public class DesignTimeDbContextFactory : IDesignTimeDbContextFactory<GymRoutineContext>
{
    public GymRoutineContext CreateDbContext(string[] args)
    {
        var optionsBuilder = new DbContextOptionsBuilder<GymRoutineContext>();
        optionsBuilder.UseSqlite("Data Source=gymroutine.db");

        return new GymRoutineContext(optionsBuilder.Options);
    }
}