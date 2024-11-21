using Microsoft.EntityFrameworkCore;
using trackit.server.Models;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
    {
    }

    public DbSet<Requirement> Requirements { get; set; }
    public DbSet<RequirementType> RequirementTypes { get; set; }
    public DbSet<Category> Categories { get; set; }
    public DbSet<RequirementRelation> RequirementRelations { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Relationship between RequirementType and Category
        modelBuilder.Entity<Category>()
            .HasOne(c => c.RequirementType)
            .WithMany(rt => rt.Categories)
            .HasForeignKey(c => c.RequirementTypeId)
            .OnDelete(DeleteBehavior.Restrict); // Cambiado a Restrict

        // Relationship between Requirement and RequirementType
        modelBuilder.Entity<Requirement>()
            .HasOne(r => r.RequirementType)
            .WithMany()
            .HasForeignKey(r => r.RequirementTypeId)
            .OnDelete(DeleteBehavior.Restrict); // Cambiado a Restrict

        // Relationship between Requirement and Category
        modelBuilder.Entity<Requirement>()
            .HasOne(r => r.Category)
            .WithMany()
            .HasForeignKey(r => r.CategoryId)
            .OnDelete(DeleteBehavior.Restrict); // Cambiado a Restrict

        // Self-referencing relationship for RequirementRelation
        modelBuilder.Entity<RequirementRelation>()
            .HasKey(rr => new { rr.RequirementId, rr.RelatedRequirementId });

        modelBuilder.Entity<RequirementRelation>()
            .HasOne(rr => rr.Requirement)
            .WithMany(r => r.RelatedRequirements)
            .HasForeignKey(rr => rr.RequirementId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<RequirementRelation>()
            .HasOne(rr => rr.RelatedRequirement)
            .WithMany()
            .HasForeignKey(rr => rr.RelatedRequirementId)
            .OnDelete(DeleteBehavior.Restrict);

        // Seed Data
        modelBuilder.Entity<RequirementType>().HasData(
            new RequirementType { Id = 1, Name = "Hardware" },
            new RequirementType { Id = 2, Name = "Software" },
            new RequirementType { Id = 3, Name = "Maintenance" }
        );

        modelBuilder.Entity<Category>().HasData(
            new Category { Id = 1, Name = "Hardware Malfunction", RequirementTypeId = 1 },
            new Category { Id = 2, Name = "Network Issue", RequirementTypeId = 1 },
            new Category { Id = 3, Name = "Software Bug", RequirementTypeId = 2 },
            new Category { Id = 4, Name = "Database Maintenance", RequirementTypeId = 2 },
            new Category { Id = 5, Name = "Routine Check", RequirementTypeId = 3 }
        );
    }

}

