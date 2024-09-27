using Microsoft.EntityFrameworkCore;

namespace DatabaseDistributedLock;

public class LockDbContext : DbContext
{
    public DbSet<LockRecord> LockRecords { get; set; }

    public LockDbContext(DbContextOptions<LockDbContext> options) : base(options) { }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<LockRecord>().ToTable($"lock_records");
        modelBuilder.Entity<LockRecord>().Property(b => b.Id).IsRequired().HasMaxLength(128);
        modelBuilder.Entity<LockRecord>().Property(b => b.Key).IsRequired().HasMaxLength(128);
        modelBuilder.Entity<LockRecord>().Property(b => b.Count).IsRequired();
        modelBuilder.Entity<LockRecord>().Property(b => b.ExpireTime).IsRequired();
        modelBuilder.Entity<LockRecord>().Property(b => b.CreationTime).IsRequired();
        modelBuilder.Entity<LockRecord>().HasKey(b => b.Key);
    }
}