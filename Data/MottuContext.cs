using backend.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace backend.Data
{
    public class MottuContext : IdentityDbContext<ApplicationUser, IdentityRole<int>, int>
    {
        public MottuContext(DbContextOptions<MottuContext> options)
            : base(options)
        {
        }
        public DbSet<Moto> Motos { get; set; }
        public DbSet<UwbTag> UwbTags { get; set; }
        public DbSet<UwbAnchor> UwbAnchors { get; set; }
        public DbSet<UwbMeasurement> UwbMeasurements { get; set; }
        public DbSet<PositionRecord> PositionRecords { get; set; }
    }
}
