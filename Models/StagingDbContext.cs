using Microsoft.EntityFrameworkCore;
using ConsoleApp.Services;

namespace ConsoleApp.Data
{
    public class StagingDbContext : DbContext
    {
        public StagingDbContext(DbContextOptions<StagingDbContext> options) : base(options) { }

        public DbSet<StagingDataModel> StagingDataModels { get; set; }
        public DbSet<MetadataStagingModel> MetadataStagingModels { get; set; }

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
                entity.Property(e => e.CreatedTimestamp).HasDefaultValueSql("GETUTCDATE()");
                entity.HasIndex(e => e.TenantId);
                entity.HasIndex(e => new { e.TenantId, e.Type });
                entity.ToTable("StagingData");
            });

            // Configure MetadataStagingModel
            modelBuilder.Entity<MetadataStagingModel>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.TenantId).IsRequired().HasMaxLength(100);
                entity.Property(e => e.Source).IsRequired().HasMaxLength(255);
                entity.Property(e => e.MetadataContent).IsRequired();
                entity.HasIndex(e => e.TenantId);
                entity.ToTable("MetadataStaging");
            });
        }
    }
}
