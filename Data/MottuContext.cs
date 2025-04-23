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
        protected override void OnModelCreating(ModelBuilder builder)
        {
            //isso é necessario ja que o oracle antes da versao 21 nao possui tipo boolean
            foreach(var entityType in builder.Model.GetEntityTypes())
            {
                foreach(var property in entityType.GetProperties())
                {
                    if(property.ClrType == typeof(bool))
                    {
                        property.SetColumnType("number(1)");
                    }
                }
            }
            base.OnModelCreating(builder);
        }
        public DbSet<Moto> Motos { get; set; }
        public DbSet<UwbTag> UwbTags { get; set; }
        public DbSet<UwbAnchor> UwbAnchors { get; set; }
        public DbSet<UwbMeasurement> UwbMeasurements { get; set; }
        public DbSet<PositionRecord> PositionRecords { get; set; }
    }
}
