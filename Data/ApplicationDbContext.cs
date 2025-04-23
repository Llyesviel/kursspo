using Microsoft.EntityFrameworkCore;
using Airport.Models;

namespace Airport.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        public DbSet<User> Users { get; set; } = null!;
        public DbSet<Aircraft> Aircrafts { get; set; } = null!;
        public DbSet<Flight> Flights { get; set; } = null!;
        public DbSet<Landing> Landings { get; set; } = null!;
        public DbSet<Ticket> Tickets { get; set; } = null!;

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Настройка для User
            modelBuilder.Entity<User>()
                .HasIndex(u => u.Email)
                .IsUnique();

            // Явное указание имен таблиц
            modelBuilder.Entity<Aircraft>().ToTable("Aircraft");
            modelBuilder.Entity<Flight>().ToTable("Flight");
            modelBuilder.Entity<Landing>().ToTable("Landing");
            modelBuilder.Entity<Ticket>().ToTable("Ticket");

            // Конфигурация отношений между моделями
            modelBuilder.Entity<Flight>()
                .HasOne(f => f.Aircraft)
                .WithMany(a => a.Flights)
                .HasForeignKey(f => f.AircraftId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Landing>()
                .HasOne(l => l.Flight)
                .WithMany(f => f.Landings)
                .HasForeignKey(l => l.FlightId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Ticket>()
                .HasOne(t => t.Flight)
                .WithMany(f => f.Tickets)
                .HasForeignKey(t => t.FlightId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
} 