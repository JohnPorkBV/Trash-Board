using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using Trash_Board.Models;

namespace Trash_Board.Data
{
    public class TrashboardDbContext : DbContext
    {
        public TrashboardDbContext(DbContextOptions<TrashboardDbContext> options) : base(options)
        {
        }

        public DbSet<TrashDetection> TrashDetections { get; set; }
    }

}
