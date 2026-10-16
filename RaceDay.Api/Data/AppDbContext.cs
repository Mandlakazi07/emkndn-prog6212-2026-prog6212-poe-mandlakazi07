// Student:     Mandlakazi Sibiya (ST10473547)
// Module:      PROG6212 Programming 2B - PoE Part 2
// Institution: The Independent Institute of Education (IIE)
// Date:        16 October 2026
// AI use:      Claude (Anthropic) helped draft and explain this file. Reviewed and understood by the student.
// Reference:   Microsoft Learn, n.d. Check constraints - EF Core. [online] Available at:
//              https://learn.microsoft.com/en-us/ef/core/modeling/constraints [Accessed 16 October 2026].

using Microsoft.EntityFrameworkCore;
using RaceDay.Api.Models;

namespace RaceDay.Api.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
    {
    }

    // Each DbSet becomes one table, named after the property
    public DbSet<User> Users => Set<User>();
    public DbSet<Venue> Venues => Set<Venue>();
    public DbSet<Event> Events => Set<Event>();
    public DbSet<Category> Categories => Set<Category>();
    public DbSet<Enrollment> Enrollments => Set<Enrollment>();
    public DbSet<Result> Results => Set<Result>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Users: UNIQUE email, role CHECK and CreatedAt DEFAULT, as in the Part 1 SQL script
        modelBuilder.Entity<User>(entity =>
        {
            entity.HasIndex(u => u.Email).IsUnique();
            entity.Property(u => u.CreatedAt).HasDefaultValueSql("GETDATE()");
            entity.ToTable(t => t.HasCheckConstraint("CK_Users_Role", "[Role] IN ('Organizer','Participant')"));
        });

        // Events: type CHECK, plus the two foreign keys (organizer and venue)
        modelBuilder.Entity<Event>(entity =>
        {
            entity.ToTable(t => t.HasCheckConstraint("CK_Events_Type", "[EventType] IN ('Run','Walk','Cycle')"));
            entity.HasOne(e => e.Organizer).WithMany(u => u.OrganizedEvents)
                  .HasForeignKey(e => e.OrganizerID).OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(e => e.Venue).WithMany(v => v.Events)
                  .HasForeignKey(e => e.VenueID).OnDelete(DeleteBehavior.Restrict);
        });

        // Categories: each belongs to one event
        modelBuilder.Entity<Category>(entity =>
        {
            entity.HasOne(c => c.Event).WithMany(e => e.Categories)
                  .HasForeignKey(c => c.EventID).OnDelete(DeleteBehavior.Restrict);
        });

        // Enrollments: status CHECK + defaults, plus three foreign keys
        modelBuilder.Entity<Enrollment>(entity =>
        {
            entity.Property(x => x.Status).HasDefaultValue("Pending");
            entity.Property(x => x.EnrollmentDate).HasDefaultValueSql("GETDATE()");
            entity.ToTable(t => t.HasCheckConstraint("CK_Enrollments_Status", "[Status] IN ('Pending','Confirmed','Cancelled')"));
            entity.HasOne(x => x.Participant).WithMany(u => u.Enrollments)
                  .HasForeignKey(x => x.ParticipantID).OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(x => x.Event).WithMany(e => e.Enrollments)
                  .HasForeignKey(x => x.EventID).OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(x => x.Category).WithMany(c => c.Enrollments)
                  .HasForeignKey(x => x.CategoryID).OnDelete(DeleteBehavior.Restrict);
        });

        // Results: strictly one result per enrollment (one-to-one, so EnrollmentID is UNIQUE)
        modelBuilder.Entity<Result>(entity =>
        {
            entity.HasOne(r => r.Enrollment).WithOne(x => x.Result)
                  .HasForeignKey<Result>(r => r.EnrollmentID).OnDelete(DeleteBehavior.Restrict);
        });
    }
}
