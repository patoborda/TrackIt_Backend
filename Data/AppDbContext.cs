using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using trackit.server.Models;

namespace trackit.server.Data
{
    public class UserDbContext : IdentityDbContext<User>
    {
        public UserDbContext(DbContextOptions<UserDbContext> options) : base(options) { }

        public DbSet<Requirement> Requirements { get; set; }
        public DbSet<RequirementType> RequirementTypes { get; set; }
        public DbSet<Category> Categories { get; set; }
        public DbSet<RequirementRelation> RequirementRelations { get; set; }
        public DbSet<Priority> Priorities { get; set; }
        public DbSet<RequirementActionLog> RequirementActionLogs { get; set; }
        public DbSet<InternalUser> InternalUsers { get; set; }
        public DbSet<ExternalUser> ExternalUsers { get; set; }
        public DbSet<AdminUser> AdminUsers { get; set; }
        public DbSet<UserRequirement> UserRequirements { get; set; }
        public DbSet<Comment> Comments { get; set; }
        public DbSet<AttachedFile> AttachedFiles { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Relación uno-a-uno entre User e InternalUser
            modelBuilder.Entity<InternalUser>()
                .HasOne(iu => iu.User)
                .WithOne(u => u.InternalUser)
                .HasForeignKey<InternalUser>(iu => iu.Id)
                .OnDelete(DeleteBehavior.Restrict);

            // Relación uno-a-uno entre User y ExternalUser
            modelBuilder.Entity<ExternalUser>()
                .HasOne(eu => eu.User)
                .WithOne(u => u.ExternalUser)
                .HasForeignKey<ExternalUser>(eu => eu.Id)
                .OnDelete(DeleteBehavior.Restrict);

            // Relación uno-a-uno entre User y AdminUser
            modelBuilder.Entity<AdminUser>()
                .HasOne(au => au.User)
                .WithOne(u => u.AdminUser)
                .HasForeignKey<AdminUser>(au => au.Id)
                .OnDelete(DeleteBehavior.Restrict);

            // Configuración de la relación muchos-a-muchos entre User y Requirement (UserRequirement)
            modelBuilder.Entity<UserRequirement>()
                .HasKey(ur => new { ur.UserId, ur.RequirementId });

            modelBuilder.Entity<UserRequirement>()
                .HasOne(ur => ur.User)
                .WithMany(u => u.UserRequirements)
                .HasForeignKey(ur => ur.UserId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<UserRequirement>()
                .HasOne(ur => ur.Requirement)
                .WithMany(r => r.UserRequirements)
                .HasForeignKey(ur => ur.RequirementId)
                .OnDelete(DeleteBehavior.Restrict);

            // Configuración de la relación auto-referenciada para RequirementRelation
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
            // Configuración de la relación con RequirementType
            modelBuilder.Entity<Requirement>()
                .HasOne(r => r.RequirementType)
                .WithMany()
                .HasForeignKey(r => r.RequirementTypeId)
                .OnDelete(DeleteBehavior.Restrict); // Cambiar a Restrict para evitar cascada

            // Configuración de la relación con Category
            modelBuilder.Entity<Requirement>()
                .HasOne(r => r.Category)
                .WithMany()
                .HasForeignKey(r => r.CategoryId)
                .OnDelete(DeleteBehavior.Restrict); // Cambiar a Restrict para evitar cascada

            // Configuración de la relación con Priority
            modelBuilder.Entity<Requirement>()
                .HasOne(r => r.Priority)
                .WithMany()
                .HasForeignKey(r => r.PriorityId)
                .OnDelete(DeleteBehavior.SetNull); // Cambiar a SetNull si PriorityId es opcional

            // Configuración de la relación Comment -> Requirement
            modelBuilder.Entity<Comment>()
                .HasOne(c => c.Requirement)
                .WithMany(r => r.Comments)
                .HasForeignKey(c => c.RequirementId)
                .OnDelete(DeleteBehavior.Cascade);

            // Configuración de la relación Comment -> User
            modelBuilder.Entity<Comment>()
                .HasOne(c => c.User)
                .WithMany(u => u.Comments)
                .HasForeignKey(c => c.UserId)
                .OnDelete(DeleteBehavior.Restrict);

            // Configuración de la relación Comment -> AttachedFile
            modelBuilder.Entity<Comment>()
                .HasMany(c => c.Files)
                .WithOne()
                .OnDelete(DeleteBehavior.Cascade);


            // Seed Data para RequirementType
            modelBuilder.Entity<RequirementType>().HasData(
                new RequirementType { Id = 1, Name = "Hardware" },
                new RequirementType { Id = 2, Name = "Software" },
                new RequirementType { Id = 3, Name = "Maintenance" }
            );

            // Seed Data para Category
            modelBuilder.Entity<Category>().HasData(
                new Category { Id = 1, Name = "Hardware Malfunction", RequirementTypeId = 1 },
                new Category { Id = 2, Name = "Network Issue", RequirementTypeId = 1 },
                new Category { Id = 3, Name = "Software Bug", RequirementTypeId = 2 },
                new Category { Id = 4, Name = "Database Maintenance", RequirementTypeId = 2 },
                new Category { Id = 5, Name = "Routine Check", RequirementTypeId = 3 }
            );

            // Seed Data para Priority
            modelBuilder.Entity<Priority>().HasData(
                new Priority { Id = 1, TypePriority = "Alta" },
                new Priority { Id = 2, TypePriority = "Media" },
                new Priority { Id = 3, TypePriority = "Baja" },
                new Priority { Id = 4, TypePriority = "Urgente" }
            );
        }
    }
}
