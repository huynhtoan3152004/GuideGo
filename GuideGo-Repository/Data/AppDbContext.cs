using GuideGo_Repository.Entities;
using GuideGo_Repository.Enums;
using Microsoft.EntityFrameworkCore;

namespace GuideGo_Repository.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    public DbSet<User> Users => Set<User>();
    public DbSet<Guide> Guides => Set<Guide>();
    public DbSet<Company> Companies => Set<Company>();
    public DbSet<Location> Locations => Set<Location>();
    public DbSet<Tour> Tours => Set<Tour>();
    public DbSet<TourImage> TourImages => Set<TourImage>();
    public DbSet<TourSchedule> TourSchedules => Set<TourSchedule>();
    public DbSet<Cart> Carts => Set<Cart>();
    public DbSet<CartItem> CartItems => Set<CartItem>();
    public DbSet<Booking> Bookings => Set<Booking>();
    public DbSet<Payment> Payments => Set<Payment>();
    public DbSet<Review> Reviews => Set<Review>();
    public DbSet<Chat> Chats => Set<Chat>();
    public DbSet<Message> Messages => Set<Message>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // ── User ──────────────────────────────────────────────────────────────
        modelBuilder.Entity<User>(e =>
        {
            e.HasKey(u => u.Id);
            e.Property(u => u.Id).HasDefaultValueSql("gen_random_uuid()");
            e.HasIndex(u => u.Email).IsUnique();
            e.Property(u => u.IsActive).HasDefaultValue(true);
            e.Property(u => u.Role)
             .HasConversion<string>()
             .HasMaxLength(20);
        });

        // ── Guide (1:1 User) ──────────────────────────────────────────────────
        modelBuilder.Entity<Guide>(e =>
        {
            e.HasKey(g => g.Id);
            e.Property(g => g.Id).HasDefaultValueSql("gen_random_uuid()");
            e.HasOne(g => g.User)
             .WithOne(u => u.Guide)
             .HasForeignKey<Guide>(g => g.UserId)
             .OnDelete(DeleteBehavior.Cascade);
            e.Property(g => g.Rating).HasColumnType("decimal(2,1)");
        });

        // ── Company (1:1 User) ────────────────────────────────────────────────
        modelBuilder.Entity<Company>(e =>
        {
            e.HasKey(c => c.Id);
            e.Property(c => c.Id).HasDefaultValueSql("gen_random_uuid()");
            e.HasOne(c => c.User)
             .WithOne(u => u.Company)
             .HasForeignKey<Company>(c => c.UserId)
             .OnDelete(DeleteBehavior.Cascade);
        });

        // ── Location ──────────────────────────────────────────────────────────
        modelBuilder.Entity<Location>(e =>
        {
            e.HasKey(l => l.Id);
            e.Property(l => l.Id).HasDefaultValueSql("gen_random_uuid()");
            e.Property(l => l.Latitude).HasColumnType("decimal(10,7)");
            e.Property(l => l.Longitude).HasColumnType("decimal(10,7)");
        });

        // ── Tour ──────────────────────────────────────────────────────────────
        modelBuilder.Entity<Tour>(e =>
        {
            e.HasKey(t => t.Id);
            e.Property(t => t.Id).HasDefaultValueSql("gen_random_uuid()");
            e.Property(t => t.PricePerPerson).HasColumnType("decimal(10,2)");
            e.Property(t => t.Rating).HasColumnType("decimal(2,1)");
            e.Property(t => t.IsActive).HasDefaultValue(true);
            e.Property(t => t.UpdatedAt).HasDefaultValueSql("now()");
            e.HasOne(t => t.Location)
             .WithMany(l => l.Tours)
             .HasForeignKey(t => t.LocationId)
             .OnDelete(DeleteBehavior.Restrict);
            e.HasOne(t => t.Company)
             .WithMany(c => c.Tours)
             .HasForeignKey(t => t.CompanyId)
             .OnDelete(DeleteBehavior.SetNull);
            e.HasOne(t => t.Guide)
             .WithMany(g => g.Tours)
             .HasForeignKey(t => t.GuideId)
             .OnDelete(DeleteBehavior.SetNull);
        });

        // ── TourImage ─────────────────────────────────────────────────────────
        modelBuilder.Entity<TourImage>(e =>
        {
            e.HasKey(ti => ti.Id);
            e.Property(ti => ti.Id).HasDefaultValueSql("gen_random_uuid()");
            e.HasOne(ti => ti.Tour)
             .WithMany(t => t.Images)
             .HasForeignKey(ti => ti.TourId)
             .OnDelete(DeleteBehavior.Cascade);
        });

        // ── TourSchedule ──────────────────────────────────────────────────────
        modelBuilder.Entity<TourSchedule>(e =>
        {
            e.HasKey(ts => ts.Id);
            e.Property(ts => ts.Id).HasDefaultValueSql("gen_random_uuid()");
            e.HasOne(ts => ts.Tour)
             .WithMany(t => t.Schedules)
             .HasForeignKey(ts => ts.TourId)
             .OnDelete(DeleteBehavior.Cascade);
        });

        // ── Cart (1:1 User) ───────────────────────────────────────────────────
        modelBuilder.Entity<Cart>(e =>
        {
            e.HasKey(c => c.Id);
            e.Property(c => c.Id).HasDefaultValueSql("gen_random_uuid()");
            e.HasOne(c => c.User)
             .WithOne(u => u.Cart)
             .HasForeignKey<Cart>(c => c.UserId)
             .OnDelete(DeleteBehavior.Cascade);
        });

        // ── CartItem ──────────────────────────────────────────────────────────
        modelBuilder.Entity<CartItem>(e =>
        {
            e.HasKey(ci => ci.Id);
            e.Property(ci => ci.Id).HasDefaultValueSql("gen_random_uuid()");
            e.HasOne(ci => ci.Cart)
             .WithMany(c => c.Items)
             .HasForeignKey(ci => ci.CartId)
             .OnDelete(DeleteBehavior.Cascade);
            e.HasOne(ci => ci.Tour)
             .WithMany(t => t.CartItems)
             .HasForeignKey(ci => ci.TourId)
             .OnDelete(DeleteBehavior.Cascade);
            e.HasOne(ci => ci.Schedule)
             .WithMany(ts => ts.CartItems)
             .HasForeignKey(ci => ci.ScheduleId)
             .OnDelete(DeleteBehavior.Restrict);
        });

        // ── Booking ───────────────────────────────────────────────────────────
        modelBuilder.Entity<Booking>(e =>
        {
            e.HasKey(b => b.Id);
            e.Property(b => b.Id).HasDefaultValueSql("gen_random_uuid()");
            e.Property(b => b.TotalPrice).HasColumnType("decimal(10,2)");
            e.Property(b => b.Status)
             .HasConversion<string>()
             .HasMaxLength(20);
            e.HasOne(b => b.User)
             .WithMany(u => u.Bookings)
             .HasForeignKey(b => b.UserId)
             .OnDelete(DeleteBehavior.Restrict);
            e.HasOne(b => b.Schedule)
             .WithMany(ts => ts.Bookings)
             .HasForeignKey(b => b.ScheduleId)
             .OnDelete(DeleteBehavior.Restrict);
        });

        // ── Payment (1:1 Booking) ─────────────────────────────────────────────
        modelBuilder.Entity<Payment>(e =>
        {
            e.HasKey(p => p.Id);
            e.Property(p => p.Id).HasDefaultValueSql("gen_random_uuid()");
            e.Property(p => p.Amount).HasColumnType("decimal(10,2)");
            e.Property(p => p.Status)
             .HasConversion<string>()
             .HasMaxLength(20);
            e.HasOne(p => p.Booking)
             .WithOne(b => b.Payment)
             .HasForeignKey<Payment>(p => p.BookingId)
             .OnDelete(DeleteBehavior.Cascade);
            e.HasIndex(p => p.BookingId).IsUnique();
        });

        // ── Review ────────────────────────────────────────────────────────────
        modelBuilder.Entity<Review>(e =>
        {
            e.HasKey(r => r.Id);
            e.Property(r => r.Id).HasDefaultValueSql("gen_random_uuid()");
            e.HasOne(r => r.Tour)
             .WithMany(t => t.Reviews)
             .HasForeignKey(r => r.TourId)
             .OnDelete(DeleteBehavior.Cascade);
            e.HasOne(r => r.User)
             .WithMany(u => u.Reviews)
             .HasForeignKey(r => r.UserId)
             .OnDelete(DeleteBehavior.Restrict);
        });

        // ── Chat ──────────────────────────────────────────────────────────────
        modelBuilder.Entity<Chat>(e =>
        {
            e.HasKey(c => c.Id);
            e.Property(c => c.Id).HasDefaultValueSql("gen_random_uuid()");
            e.HasOne(c => c.User)
             .WithMany(u => u.Chats)
             .HasForeignKey(c => c.UserId)
             .OnDelete(DeleteBehavior.Restrict);
            e.HasOne(c => c.Guide)
             .WithMany(g => g.Chats)
             .HasForeignKey(c => c.GuideId)
             .OnDelete(DeleteBehavior.Restrict);
        });

        // ── Message ───────────────────────────────────────────────────────────
        modelBuilder.Entity<Message>(e =>
        {
            e.HasKey(m => m.Id);
            e.Property(m => m.Id).HasDefaultValueSql("gen_random_uuid()");
            e.HasOne(m => m.Chat)
             .WithMany(c => c.Messages)
             .HasForeignKey(m => m.ChatId)
             .OnDelete(DeleteBehavior.Cascade);
            e.HasOne(m => m.Sender)
             .WithMany(u => u.SentMessages)
             .HasForeignKey(m => m.SenderId)
             .OnDelete(DeleteBehavior.Restrict);
        });
    }
}
