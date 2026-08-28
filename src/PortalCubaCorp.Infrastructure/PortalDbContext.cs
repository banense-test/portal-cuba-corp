using Microsoft.EntityFrameworkCore;
using PortalCubaCorp.Domain;

namespace PortalCubaCorp.Infrastructure;

/// <summary>
/// EF Core DbContext for Portal Cuba Corp (COMP-006).
/// Maps 4 tables: clockings (T1), news_items (T2), worker_categories (T3), audit_records (T4).
/// PostgreSQL via Npgsql provider (CON-003).
/// </summary>
public class PortalDbContext : DbContext
{
    public PortalDbContext(DbContextOptions<PortalDbContext> options) : base(options)
    {
    }

    // T1: clockings
    public DbSet<ClockingRecord> Clockings => Set<ClockingRecord>();

    // T2: news_items
    public DbSet<NewsItem> NewsItems => Set<NewsItem>();

    // T3: worker_categories
    public DbSet<WorkerCategory> WorkerCategories => Set<WorkerCategory>();

    // T4: audit_records
    public DbSet<AuditRecord> AuditRecords => Set<AuditRecord>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // T1: clockings
        modelBuilder.Entity<ClockingRecord>(entity =>
        {
            entity.ToTable("clockings");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).UseIdentityByDefaultColumn();
            entity.Property(e => e.EmployeeId).IsRequired().HasMaxLength(256);
            entity.Property(e => e.Timestamp).IsRequired();
            entity.Property(e => e.Type).HasConversion<string>().IsRequired();
            entity.Property(e => e.IdempotencyKey).IsRequired().HasMaxLength(64);
            entity.HasIndex(e => e.IdempotencyKey).IsUnique(); // AC-005 idempotency
            entity.HasIndex(e => new { e.EmployeeId, e.Timestamp });
        });

        // T2: news_items
        modelBuilder.Entity<NewsItem>(entity =>
        {
            entity.ToTable("news_items");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).HasDefaultValueSql("gen_random_uuid()");
            entity.Property(e => e.Title).IsRequired().HasMaxLength(500);
            entity.Property(e => e.Body).IsRequired();
            entity.Property(e => e.Category).HasConversion<string>().IsRequired();
            entity.Property(e => e.Status).HasConversion<string>().IsRequired();
            entity.Property(e => e.IsFeatured).IsRequired();
            entity.Property(e => e.CreatedAt).IsRequired();
            entity.Property(e => e.UpdatedAt).IsRequired();
            entity.Property(e => e.AuthorId).IsRequired().HasMaxLength(256);
            entity.HasIndex(e => e.Status);
            entity.HasIndex(e => e.CreatedAt);
        });

        // T3: worker_categories — only 2 columns (CON-009)
        modelBuilder.Entity<WorkerCategory>(entity =>
        {
            entity.ToTable("worker_categories");
            entity.HasKey(e => e.AdUserId);
            entity.Property(e => e.AdUserId).IsRequired().HasMaxLength(256);
            entity.Property(e => e.Category).IsRequired().HasMaxLength(100);
        });

        // T4: audit_records — append-only (NFR-004)
        modelBuilder.Entity<AuditRecord>(entity =>
        {
            entity.ToTable("audit_records");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).UseIdentityByDefaultColumn();
            entity.Property(e => e.EntityType).IsRequired().HasMaxLength(50);
            entity.Property(e => e.EntityId).IsRequired().HasMaxLength(256);
            entity.Property(e => e.Action).HasConversion<string>().IsRequired();
            entity.Property(e => e.Author).IsRequired().HasMaxLength(256);
            entity.Property(e => e.Timestamp).IsRequired();
            entity.HasIndex(e => new { e.EntityType, e.EntityId });
        });
    }
}