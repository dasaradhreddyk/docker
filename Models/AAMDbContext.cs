using Microsoft.EntityFrameworkCore;
using ConsoleApp.Services;

namespace ConsoleApp.Data
{
    public class AAMDbContext : DbContext
    {
        public AAMDbContext(DbContextOptions<AAMDbContext> options) : base(options) { }

        public DbSet<AAMDataModel> AAMDataModels { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Configure AAMDataModel
            modelBuilder.Entity<AAMDataModel>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.TenantId).IsRequired().HasMaxLength(100);
                entity.Property(e => e.Type).IsRequired().HasMaxLength(50);
                entity.Property(e => e.Status).IsRequired().HasMaxLength(50);
                entity.Property(e => e.TransformationTimestamp).HasDefaultValueSql("GETUTCDATE()");
                entity.HasIndex(e => e.TenantId);
                entity.HasIndex(e => new { e.TenantId, e.Status });
                entity.ToTable("AAMData");
            });
        }
    }
}
