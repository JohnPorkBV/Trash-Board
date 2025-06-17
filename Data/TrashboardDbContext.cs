using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using TrashBoard.Models;

namespace TrashBoard.Data
{
    public class TrashboardDbContext : DbContext
    {
        public TrashboardDbContext(DbContextOptions<TrashboardDbContext> options) : base(options)
        {
        }

        public DbSet<TrashDetection> TrashDetections { get; set; }
        public DbSet<BredaEvent> BredaEvents { get; set; }
        public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            foreach (var entry in ChangeTracker.Entries<BredaEvent>())
            {
                if (entry.State == EntityState.Added || entry.State == EntityState.Modified)
                {
                    if (!entry.Entity.EndDate.HasValue)
                    {
                        entry.Entity.EndDate = entry.Entity.StartDate;
                    }
                }
            }

            return await base.SaveChangesAsync(cancellationToken);
        }
    }
}
