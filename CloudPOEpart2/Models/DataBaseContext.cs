using CloudPOEpart2.Models;
using Microsoft.EntityFrameworkCore;

public class DataBaseContext : DbContext
{
    public DataBaseContext(DbContextOptions<DataBaseContext> options) : base(options)
    {
    }

    public DbSet<Venue> Venue { get; set; }
    public DbSet<Event> Event { get; set; }
    public DbSet<Booking> Booking { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Booking>()
            .HasOne(b => b.Event)
            .WithMany()
            .HasForeignKey(b => b.EventID)
            .OnDelete(DeleteBehavior.Cascade); // keep cascade for Event

        modelBuilder.Entity<Booking>()
            .HasOne(b => b.Venue)
            .WithMany()
            .HasForeignKey(b => b.VenueID)
            .OnDelete(DeleteBehavior.Restrict); // prevent cascade for Venue
    }
}
