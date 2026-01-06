using Microsoft.EntityFrameworkCore;
using DoAn.Core.Domain.Entities;

namespace DoAn.Infrastructure.Data;

public class ApplicationDbContext : DbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options) { }

    // DbSets (expression-bodied, tránh setter)
    public DbSet<User> Users => Set<User>();
    public DbSet<Role> Roles => Set<Role>();
    public DbSet<Permission> Permissions => Set<Permission>();
    public DbSet<RolePermission> RolePermissions => Set<RolePermission>();
    public DbSet<UserRole> UserRoles => Set<UserRole>();
    public DbSet<RefreshToken> RefreshTokens => Set<RefreshToken>();
    public DbSet<Complex> Complexes => Set<Complex>();
    public DbSet<Field> Fields => Set<Field>();
    public DbSet<TimeSlot> TimeSlots => Set<TimeSlot>();
    public DbSet<FavoriteComplex> FavoriteComplexes => Set<FavoriteComplex>();
    public DbSet<Booking> Bookings => Set<Booking>();
    public DbSet<Review> Reviews => Set<Review>();
    public DbSet<ReviewImage> ReviewImages => Set<ReviewImage>();
    public DbSet<ReviewHelpfulVote> ReviewHelpfulVotes => Set<ReviewHelpfulVote>();
    public DbSet<ComplexImage> ComplexImages => Set<ComplexImage>();
    public DbSet<OwnerSetting> OwnerSettings => Set<OwnerSetting>();
    public DbSet<SystemConfig> SystemConfigs => Set<SystemConfig>();
    public DbSet<UserActivityLog> UserActivityLogs => Set<UserActivityLog>();
    public DbSet<SystemLog> SystemLogs => Set<SystemLog>();
    public DbSet<Notification> Notifications => Set<Notification>();
    public DbSet<Province> Provinces => Set<Province>();
    public DbSet<Ward> Wards => Set<Ward>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Apply all entity configurations from the assembly
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(ApplicationDbContext).Assembly);
    }

    public override int SaveChanges()
    {
        UpdateTimestamps();
        return base.SaveChanges();
    }

    public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        UpdateTimestamps();
        return base.SaveChangesAsync(cancellationToken);
    }

    private void UpdateTimestamps()
    {
        var entries = ChangeTracker.Entries()
            .Where(e => e.State == EntityState.Added || e.State == EntityState.Modified);

        var vietnamTimeZone = TimeZoneInfo.FindSystemTimeZoneById("SE Asia Standard Time");
        var vietnamNow = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, vietnamTimeZone);

        foreach (var entry in entries)
        {
            var updatedAtProperty = entry.Properties.FirstOrDefault(p => p.Metadata.Name == "UpdatedAt");
            if (updatedAtProperty != null)
                updatedAtProperty.CurrentValue = vietnamNow;

            if (entry.State == EntityState.Added)
            {
                var createdAtProperty = entry.Properties.FirstOrDefault(p => p.Metadata.Name == "CreatedAt");
                if (createdAtProperty != null)
                    createdAtProperty.CurrentValue = vietnamNow;
            }
        }
    }
}
