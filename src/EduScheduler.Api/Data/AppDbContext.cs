using EduScheduler.Api.Models.Entities;
using Microsoft.EntityFrameworkCore;

namespace EduScheduler.Api.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    public DbSet<AppUser> AppUsers => Set<AppUser>();
    public DbSet<Student> Students => Set<Student>();
    public DbSet<StudentEvent> StudentEvents => Set<StudentEvent>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<AppUser>(entity =>
        {
            entity.HasIndex(e => e.Username).IsUnique();
            entity.HasIndex(e => e.Email).IsUnique();
        });

        modelBuilder.Entity<Student>(entity =>
        {
            entity.HasIndex(e => e.GraphId).IsUnique();
            entity.HasIndex(e => e.Email);
            entity.HasIndex(e => e.DisplayName);
        });

        modelBuilder.Entity<StudentEvent>(entity =>
        {
            entity.HasIndex(e => e.GraphEventId);
            entity.HasIndex(e => e.StudentId);

            entity.HasOne(e => e.Student)
                .WithMany(s => s.Events)
                .HasForeignKey(e => e.StudentId)
                .OnDelete(DeleteBehavior.Cascade);
        });
    }

}
