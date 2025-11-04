using Microsoft.EntityFrameworkCore;
using FootballField.API.Entities;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace FootballField.API.DbContexts;

public class ApplicationDbContext : DbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options) { }

    // DbSets (expression-bodied, tránh setter)
    public DbSet<User> Users => Set<User>();
    public DbSet<Complex> Complexes => Set<Complex>();
    public DbSet<Field> Fields => Set<Field>();
    public DbSet<TimeSlot> TimeSlots => Set<TimeSlot>();
    public DbSet<FavoriteComplex> FavoriteComplexes => Set<FavoriteComplex>();
    public DbSet<Booking> Bookings => Set<Booking>();
    public DbSet<Review> Reviews => Set<Review>();
    public DbSet<ComplexImage> ComplexImages => Set<ComplexImage>();
    public DbSet<OwnerSetting> OwnerSettings => Set<OwnerSetting>();
    public DbSet<SystemConfig> SystemConfigs => Set<SystemConfig>();
    public DbSet<UserActivityLog> UserActivityLogs => Set<UserActivityLog>();
    public DbSet<SystemLog> SystemLogs => Set<SystemLog>();
    public DbSet<Notification> Notifications => Set<Notification>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // ======================= USER =======================
        modelBuilder.Entity<User>(entity =>
        {
            entity.ToTable("USER");

            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).HasColumnName("id");

            entity.Property(e => e.LastName).HasColumnName("last_name").HasMaxLength(100).IsUnicode(true).IsRequired();
            entity.Property(e => e.FirstName).HasColumnName("first_name").HasMaxLength(100).IsUnicode(true).IsRequired();
            entity.Property(e => e.Email).HasColumnName("email").HasMaxLength(200);
            entity.Property(e => e.Phone).HasColumnName("phone").HasMaxLength(15);
            entity.Property(e => e.Password).HasColumnName("password").HasMaxLength(255);
            entity.Property(e => e.Role).HasColumnName("role").IsRequired();
            entity.Property(e => e.AvatarUrl).HasColumnName("avatar_url").IsUnicode(false);
            entity.Property(e => e.Status).HasColumnName("status").IsRequired();
            entity.Property(e => e.IsDeleted).HasColumnName("is_deleted").HasDefaultValue(false);
            entity.Property(e => e.DeletedBy).HasColumnName("deleted_by");
            entity.Property(e => e.CreatedAt).HasColumnName("created_at").HasDefaultValueSql("GETDATE()");
            entity.Property(e => e.UpdatedAt).HasColumnName("updated_at").HasDefaultValueSql("GETDATE()");
            entity.Property(e => e.DeletedAt).HasColumnName("deleted_at");

            entity.HasIndex(e => e.Email).IsUnique();

            entity.HasOne(e => e.DeletedByUser)
                .WithMany()
                .HasForeignKey(e => e.DeletedBy)
                .OnDelete(DeleteBehavior.NoAction);

            entity.HasOne(e => e.OwnerSetting)
                .WithOne(e => e.Owner)
                .HasForeignKey<OwnerSetting>(e => e.OwnerId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        // ======================= COMPLEX =======================
        modelBuilder.Entity<Complex>(entity =>
        {
            entity.ToTable("COMPLEX");

            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).HasColumnName("id");

            entity.Property(e => e.OwnerId).HasColumnName("owner_id").IsRequired();
            entity.Property(e => e.Name).HasColumnName("name").HasMaxLength(255).IsUnicode(true).IsRequired();
            entity.Property(e => e.Street).HasColumnName("street").HasMaxLength(100).IsUnicode(true);
            entity.Property(e => e.Ward).HasColumnName("ward").HasMaxLength(100).IsUnicode(true);
            entity.Property(e => e.Province).HasColumnName("province").HasMaxLength(100).IsUnicode(true);
            entity.Property(e => e.Phone).HasColumnName("phone").HasMaxLength(15);
            entity.Property(e => e.OpeningTime).HasColumnName("opening_time");
            entity.Property(e => e.ClosingTime).HasColumnName("closing_time");
            entity.Property(e => e.Description).HasColumnName("description").HasMaxLength(500).IsUnicode(true);
            entity.Property(e => e.Status).HasColumnName("status").IsRequired();
            entity.Property(e => e.IsActive).HasColumnName("is_active").HasDefaultValue(true);
            entity.Property(e => e.IsDeleted).HasColumnName("is_deleted").HasDefaultValue(false);
            entity.Property(e => e.CreatedAt).HasColumnName("created_at").HasDefaultValueSql("GETDATE()");
            entity.Property(e => e.UpdatedAt).HasColumnName("updated_at").HasDefaultValueSql("GETDATE()");
            entity.Property(e => e.DeletedAt).HasColumnName("deleted_at");

            entity.HasIndex(e => e.OwnerId).HasDatabaseName("IX_Complex_OwnerId");

            entity.HasOne(e => e.Owner)
                .WithMany(e => e.OwnedComplexes)
                .HasForeignKey(e => e.OwnerId)
                .OnDelete(DeleteBehavior.NoAction);
        });

        // ======================= FIELD =======================
        modelBuilder.Entity<Field>(entity =>
        {
            entity.ToTable("FIELD");

            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).HasColumnName("id");

            entity.Property(e => e.ComplexId).HasColumnName("complex_id").IsRequired();
            entity.Property(e => e.Name).HasColumnName("name").HasMaxLength(100).IsUnicode(true).IsRequired();
            entity.Property(e => e.SurfaceType).HasColumnName("surface_type").HasMaxLength(50).IsUnicode(true);
            entity.Property(e => e.FieldSize).HasColumnName("field_size").HasMaxLength(50).IsUnicode(true);
            entity.Property(e => e.IsActive).HasColumnName("is_active").HasDefaultValue(true);
            entity.Property(e => e.IsDeleted).HasColumnName("is_deleted").HasDefaultValue(false);
            entity.Property(e => e.CreatedAt).HasColumnName("created_at").HasDefaultValueSql("GETDATE()");
            entity.Property(e => e.UpdatedAt).HasColumnName("updated_at").HasDefaultValueSql("GETDATE()");
            entity.Property(e => e.DeletedAt).HasColumnName("deleted_at");

            entity.HasIndex(e => e.ComplexId).HasDatabaseName("IX_Field_ComplexId");

            entity.HasOne(e => e.Complex)
                .WithMany(e => e.Fields)
                .HasForeignKey(e => e.ComplexId)
                .OnDelete(DeleteBehavior.NoAction);
        });

        // ======================= TIME SLOT =======================
        modelBuilder.Entity<TimeSlot>(entity =>
        {
            entity.ToTable("TIME_SLOT");

            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).HasColumnName("id");

            entity.Property(e => e.FieldId).HasColumnName("field_id").IsRequired();
            entity.Property(e => e.StartTime).HasColumnName("start_time").IsRequired();
            entity.Property(e => e.EndTime).HasColumnName("end_time").IsRequired();
            entity.Property(e => e.Price).HasColumnName("price").HasColumnType("decimal(10,2)").IsRequired();
            entity.Property(e => e.IsActive).HasColumnName("is_active").HasDefaultValue(true);
            entity.Property(e => e.CreatedAt).HasColumnName("created_at").HasDefaultValueSql("GETDATE()");
            entity.Property(e => e.UpdatedAt).HasColumnName("updated_at").HasDefaultValueSql("GETDATE()");

            entity.HasIndex(e => e.FieldId).HasDatabaseName("IX_TimeSlot_FieldId");
            entity.HasIndex(e => new { e.FieldId, e.StartTime, e.EndTime }).IsUnique();

            // CHECK: start_time < end_time
            entity.ToTable(tb =>
            {
                tb.HasCheckConstraint("CK_TimeSlot_TimeRange", "start_time < end_time");
            });

            entity.HasOne(e => e.Field)
                .WithMany(e => e.TimeSlots)
                .HasForeignKey(e => e.FieldId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        // ======================= FAVORITE FIELD =======================
        modelBuilder.Entity<FavoriteComplex>(entity =>
        {
            entity.ToTable("FAVORITE_COMPLEX");

            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).HasColumnName("id");

            entity.Property(e => e.UserId).HasColumnName("user_id").IsRequired();
            entity.Property(e => e.ComplexId).HasColumnName("complex_id").IsRequired();
            entity.Property(e => e.CreatedAt).HasColumnName("created_at").HasDefaultValueSql("GETDATE()");

            entity.HasIndex(e => new { e.UserId, e.ComplexId }).IsUnique();

            entity.HasOne(e => e.User)
                .WithMany(e => e.FavoriteComplexes)
                .HasForeignKey(e => e.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(e => e.Field)
                .WithMany(e => e.FavoritedBy)
                .HasForeignKey(e => e.ComplexId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        // ======================= BOOKING =======================
        modelBuilder.Entity<Booking>(entity =>
        {
            entity.ToTable("BOOKING");

            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).HasColumnName("id");

            entity.Property(e => e.FieldId).HasColumnName("field_id").IsRequired();
            entity.Property(e => e.CustomerId).HasColumnName("customer_id").IsRequired();
            entity.Property(e => e.OwnerId).HasColumnName("owner_id").IsRequired();
            entity.Property(e => e.BookingDate).HasColumnName("booking_date").IsRequired();
            entity.Property(e => e.TimeSlotId).HasColumnName("time_slot_id").IsRequired();
            entity.Property(e => e.DepositAmount).HasColumnName("deposit_amount").IsRequired();
            entity.Property(e => e.TotalAmount).HasColumnName("total_amount").IsRequired();
            entity.Property(e => e.BookingStatus).HasColumnName("booking_status").IsRequired();
            entity.Property(e => e.PaymentStatus).HasColumnName("payment_status").IsRequired();
            entity.Property(e => e.PaymentMethod).HasColumnName("payment_method").HasMaxLength(50).IsUnicode(true);
            entity.Property(e => e.TransactionId).HasColumnName("transaction_id").HasMaxLength(50);
            entity.Property(e => e.Note).HasColumnName("note").HasMaxLength(255).IsUnicode(true);
            entity.Property(e => e.CancelledAt).HasColumnName("cancelled_at");
            entity.Property(e => e.CancelledBy).HasColumnName("cancelled_by");
            entity.Property(e => e.CreatedAt).HasColumnName("created_at").HasDefaultValueSql("GETDATE()");
            entity.Property(e => e.UpdatedAt).HasColumnName("updated_at").HasDefaultValueSql("GETDATE()");

            // CHECK constraints
            entity.ToTable(tb =>
            {
                tb.HasCheckConstraint("CK_Booking_Amount", "deposit_amount >= 0 AND total_amount >= deposit_amount");
                tb.HasCheckConstraint("CK_Booking_Status", "booking_status BETWEEN 0 AND 4");
                tb.HasCheckConstraint("CK_Payment_Status", "payment_status BETWEEN 0 AND 3");
                tb.HasCheckConstraint("CK_Booking_Date_Future", "booking_date >= GETDATE()");
            });

            // Indexes
            entity.HasIndex(e => new { e.FieldId, e.BookingDate, e.TimeSlotId }).IsUnique();
            entity.HasIndex(e => e.CustomerId).HasDatabaseName("IX_Booking_CustomerId");
            entity.HasIndex(e => e.OwnerId).HasDatabaseName("IX_Booking_OwnerId");
            entity.HasIndex(e => new { e.BookingDate, e.BookingStatus }).HasDatabaseName("IX_Booking_BookingDate_Status");

            // Relations
            entity.HasOne(e => e.Field)
                .WithMany(e => e.Bookings)
                .HasForeignKey(e => e.FieldId)
                .OnDelete(DeleteBehavior.NoAction);

            entity.HasOne(e => e.Customer)
                .WithMany(e => e.CustomerBookings)
                .HasForeignKey(e => e.CustomerId)
                .OnDelete(DeleteBehavior.NoAction);

            entity.HasOne(e => e.Owner)
                .WithMany(e => e.OwnerBookings)
                .HasForeignKey(e => e.OwnerId)
                .OnDelete(DeleteBehavior.NoAction);

            entity.HasOne(e => e.TimeSlot)
                .WithMany(e => e.Bookings)
                .HasForeignKey(e => e.TimeSlotId)
                .OnDelete(DeleteBehavior.NoAction);

            entity.HasOne(e => e.CancelledByUser)
                .WithMany()
                .HasForeignKey(e => e.CancelledBy)
                .OnDelete(DeleteBehavior.NoAction);
        });

        // ======================= REVIEW =======================
        modelBuilder.Entity<Review>(entity =>
        {
            entity.ToTable("REVIEW");

            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).HasColumnName("id");

            entity.Property(e => e.BookingId).HasColumnName("booking_id").IsRequired();
            entity.Property(e => e.CustomerId).HasColumnName("customer_id").IsRequired();
            entity.Property(e => e.ComplexId).HasColumnName("complex_id").IsRequired();
            entity.Property(e => e.Rating).HasColumnName("rating").IsRequired();
            entity.Property(e => e.Comment).HasColumnName("comment").HasMaxLength(255).IsUnicode(true);
            entity.Property(e => e.IsVisible).HasColumnName("is_visible").HasDefaultValue(true);
            entity.Property(e => e.IsDeleted).HasColumnName("is_deleted").HasDefaultValue(false);
            entity.Property(e => e.CreatedAt).HasColumnName("created_at").HasDefaultValueSql("GETDATE()");
            entity.Property(e => e.UpdatedAt).HasColumnName("updated_at").HasDefaultValueSql("GETDATE()");
            entity.Property(e => e.DeletedAt).HasColumnName("deleted_at");

            entity.HasIndex(e => e.ComplexId).HasDatabaseName("IX_Review_ComplexId");

            entity.ToTable(tb =>
            {
                tb.HasCheckConstraint("CK_Review_Rating", "rating BETWEEN 1 AND 5");
            });


            entity.HasOne(e => e.Booking)
                .WithMany(e => e.Reviews)
                .HasForeignKey(e => e.BookingId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(e => e.Customer)
                .WithMany(e => e.Reviews)
                .HasForeignKey(e => e.CustomerId)
                .OnDelete(DeleteBehavior.NoAction);

            entity.HasOne(e => e.Complex)
                .WithMany(e => e.Reviews)
                .HasForeignKey(e => e.ComplexId)
                .OnDelete(DeleteBehavior.NoAction);
        });

        // ======================= COMPLEX IMAGE =======================
        modelBuilder.Entity<ComplexImage>(entity =>
        {
            entity.ToTable("COMPLEX_IMAGE");

            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).HasColumnName("id");

            entity.Property(e => e.ComplexId).HasColumnName("complex_id").IsRequired();
            entity.Property(e => e.ImageUrl).HasColumnName("image_url").IsRequired().IsUnicode(false);
            entity.Property(e => e.IsMain).HasColumnName("is_main").HasDefaultValue(false);

            entity.HasOne(e => e.Complex)
                .WithMany(e => e.ComplexImages)
                .HasForeignKey(e => e.ComplexId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        // ======================= OWNER SETTING =======================
        modelBuilder.Entity<OwnerSetting>(entity =>
        {
            entity.ToTable("OWNER_SETTING");

            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).HasColumnName("id");

            entity.Property(e => e.OwnerId).HasColumnName("owner_id").IsRequired();
            entity.Property(e => e.DepositRate).HasColumnName("deposit_rate").HasColumnType("decimal(5,2)");
            entity.Property(e => e.MinBookingNotice).HasColumnName("min_booking_notice");
            entity.Property(e => e.AllowReview).HasColumnName("allow_review").HasDefaultValue(true);
            entity.Property(e => e.CreatedAt).HasColumnName("created_at").HasDefaultValueSql("GETDATE()");
            entity.Property(e => e.UpdatedAt).HasColumnName("updated_at").HasDefaultValueSql("GETDATE()");

            entity.HasIndex(e => e.OwnerId).IsUnique();

            // CHECKs
            entity.ToTable(tb =>
            {
                tb.HasCheckConstraint("CK_OwnerSetting_DepositRate", "deposit_rate BETWEEN 0 AND 1");
                tb.HasCheckConstraint("CK_OwnerSetting_MinBookingNotice", "min_booking_notice >= 0");
            });
        });

        // ======================= SYSTEM CONFIG =======================
        modelBuilder.Entity<SystemConfig>(entity =>
        {
            entity.ToTable("SYSTEM_CONFIG");

            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).HasColumnName("id");

            entity.Property(e => e.ConfigKey).HasColumnName("config_key").HasMaxLength(100).IsUnicode(true).IsRequired();
            entity.Property(e => e.ConfigValue).HasColumnName("config_value").HasMaxLength(255).IsUnicode(true);
            entity.Property(e => e.DataType).HasColumnName("data_type").HasMaxLength(20).HasDefaultValue("string").IsUnicode(false);
            entity.Property(e => e.Description).HasColumnName("description").HasMaxLength(255).IsUnicode(true);
            entity.Property(e => e.UpdatedAt).HasColumnName("updated_at").HasDefaultValueSql("GETDATE()");

            entity.HasIndex(e => e.ConfigKey).IsUnique();

            entity.ToTable(tb =>
            {
                tb.HasCheckConstraint("CK_SystemConfig_DataType",
                    "data_type IN ('string','int','decimal','boolean','json')");
            });
        });

        // ======================= USER ACTIVITY LOG (bigint) =======================
        modelBuilder.Entity<UserActivityLog>(entity =>
        {
            entity.ToTable("USER_ACTIVITY_LOG");

            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).HasColumnName("id"); // long/bigint

            entity.Property(e => e.UserId).HasColumnName("user_id");
            entity.Property(e => e.Action).HasColumnName("action").HasMaxLength(100).IsUnicode(true);
            entity.Property(e => e.TargetTable).HasColumnName("target_table").HasMaxLength(100).IsUnicode(true);
            entity.Property(e => e.TargetId).HasColumnName("target_id");
            entity.Property(e => e.Description).HasColumnName("description").HasMaxLength(500).IsUnicode(true);
            entity.Property(e => e.CreatedAt).HasColumnName("created_at").HasDefaultValueSql("GETDATE()");

            entity.HasOne<User>()
                .WithMany()
                .HasForeignKey(e => e.UserId)
                .OnDelete(DeleteBehavior.NoAction);
        });

        // ======================= SYSTEM LOG (bigint) =======================
        modelBuilder.Entity<SystemLog>(entity =>
        {
            entity.ToTable("SYSTEM_LOG");

            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).HasColumnName("id"); // long/bigint

            entity.Property(e => e.LogLevel).HasColumnName("log_level").HasMaxLength(20).IsUnicode(false);
            entity.Property(e => e.Source).HasColumnName("source").HasMaxLength(100).IsUnicode(true);
            entity.Property(e => e.Message).HasColumnName("message").IsUnicode(true);
            entity.Property(e => e.CreatedAt).HasColumnName("created_at").HasDefaultValueSql("GETDATE()");
        });

        // ======================= NOTIFICATION =======================
        modelBuilder.Entity<Notification>(entity =>
        {
            entity.ToTable("NOTIFICATION");

            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).HasColumnName("id");

            entity.Property(e => e.UserId).HasColumnName("user_id").IsRequired();
            entity.Property(e => e.SenderId).HasColumnName("sender_id");
            entity.Property(e => e.Title).HasColumnName("title").HasMaxLength(255).IsUnicode(true);
            entity.Property(e => e.Message).HasColumnName("message").IsUnicode(true);
            entity.Property(e => e.Type).HasColumnName("type").IsRequired();
            entity.Property(e => e.RelatedTable).HasColumnName("related_table").HasMaxLength(100).IsUnicode(true);
            entity.Property(e => e.RelatedId).HasColumnName("related_id");
            entity.Property(e => e.IsRead).HasColumnName("is_read").HasDefaultValue(false);
            entity.Property(e => e.CreatedAt).HasColumnName("created_at").HasDefaultValueSql("GETDATE()");
            entity.Property(e => e.ReadAt).HasColumnName("read_at");

            entity.HasIndex(e => new { e.UserId, e.IsRead }).HasDatabaseName("IX_Notification_UserId_IsRead");

            entity.HasOne(e => e.User)
                .WithMany(e => e.ReceivedNotifications)
                .HasForeignKey(e => e.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(e => e.Sender)
                .WithMany(e => e.SentNotifications)
                .HasForeignKey(e => e.SenderId)
                .OnDelete(DeleteBehavior.NoAction);
        });

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

        foreach (var entry in entries)
        {
            var updatedAtProperty = entry.Properties.FirstOrDefault(p => p.Metadata.Name == "UpdatedAt");
            if (updatedAtProperty != null)
                updatedAtProperty.CurrentValue = DateTime.Now;

            if (entry.State == EntityState.Added)
            {
                var createdAtProperty = entry.Properties.FirstOrDefault(p => p.Metadata.Name == "CreatedAt");
                if (createdAtProperty != null)
                    createdAtProperty.CurrentValue = DateTime.Now;
            }
        }
    }
}
