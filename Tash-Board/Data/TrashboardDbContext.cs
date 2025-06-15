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
    }

}
