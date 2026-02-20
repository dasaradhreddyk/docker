using Microsoft.EntityFrameworkCore;
using ConsoleApp.Services;

namespace ConsoleApp.Data
{
    public class AAMDbContext : DbContext
    {
        public AAMDbContext(DbContextOptions<AAMDbContext> options) : base(options) { }

        public DbSet<StagingDataModel> StagingDataModels { get; set; }
        public DbSet<MetadataStagingModel> MetadataStagingModels { get; set; }
        public DbSet<AAMDataModel> AAMDataModels { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Configure StagingDataModel
            modelBuilder.Entity<StagingDataModel>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.TenantId).IsRequired().HasMaxLength(100);
                entity.Property(e => e.Type).IsRequired().HasMaxLength(50);
                entity.Property(e => e.Data).IsRequired();
                entity.HasIndex(e => e.TenantId);
                entity.HasIndex(e => new { e.TenantId, e.Type });
            });

            // Configure MetadataStagingModel
            modelBuilder.Entity<MetadataStagingModel>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.TenantId).IsRequired().HasMaxLength(100);
                entity.Property(e => e.Source).IsRequired().HasMaxLength(255);
                entity.HasIndex(e => e.TenantId);
            });

            // Configure AAMDataModel
            modelBuilder.Entity<AAMDataModel>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.TenantId).IsRequired().HasMaxLength(100);
                entity.Property(e => e.Type).IsRequired().HasMaxLength(50);
                entity.Property(e => e.Status).IsRequired().HasMaxLength(50);
                entity.HasIndex(e => e.TenantId);
                entity.HasIndex(e => new { e.TenantId, e.Status });
            });
        }
    }
}
