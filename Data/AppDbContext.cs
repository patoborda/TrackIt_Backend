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
        public DbSet<Attachment> Attachments { get; set; }
        public DbSet<Notification> Notifications { get; set; }
        public DbSet<UserNotification> UserNotifications { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Filtro global para eliminaciones lógicas
            modelBuilder.Entity<Requirement>().HasQueryFilter(r => !r.IsDeleted);

            // Configuración de la relación uno-a-uno User -> InternalUser
            modelBuilder.Entity<InternalUser>()
                .HasOne(iu => iu.User)
                .WithOne(u => u.InternalUser)
                .HasForeignKey<InternalUser>(iu => iu.Id)
                .OnDelete(DeleteBehavior.Restrict);

            // Configuración de la relación uno-a-uno User -> ExternalUser
            modelBuilder.Entity<ExternalUser>()
                .HasOne(eu => eu.User)
                .WithOne(u => u.ExternalUser)
                .HasForeignKey<ExternalUser>(eu => eu.Id)
                .OnDelete(DeleteBehavior.Restrict);

            // Configuración de la relación uno-a-uno User -> AdminUser
            modelBuilder.Entity<AdminUser>()
                .HasOne(au => au.User)
                .WithOne(u => u.AdminUser)
                .HasForeignKey<AdminUser>(au => au.Id)
                .OnDelete(DeleteBehavior.Restrict);

            // Configuración de la relación muchos-a-muchos User -> Requirement
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
                .OnDelete(DeleteBehavior.Restrict)
                .IsRequired(false);

            // Configuración de la relación auto-referenciada RequirementRelation -> Requirement
            modelBuilder.Entity<RequirementRelation>()
                .HasKey(rr => new { rr.RequirementId, rr.RelatedRequirementId });

            modelBuilder.Entity<RequirementRelation>()
                .HasOne(rr => rr.Requirement)
                .WithMany(r => r.RelatedRequirements)
                .HasForeignKey(rr => rr.RequirementId)
                .OnDelete(DeleteBehavior.Restrict)
                .IsRequired(false);

            modelBuilder.Entity<RequirementRelation>()
                .HasOne(rr => rr.RelatedRequirement)
                .WithMany()
                .HasForeignKey(rr => rr.RelatedRequirementId)
                .OnDelete(DeleteBehavior.Restrict)
                .IsRequired(false);

            // Configuración de la relación Requirement -> RequirementType
            modelBuilder.Entity<Requirement>()
                .HasOne(r => r.RequirementType)
                .WithMany()
                .HasForeignKey(r => r.RequirementTypeId)
                .OnDelete(DeleteBehavior.Restrict);

            // Configuración de la relación Requirement -> Category
            modelBuilder.Entity<Requirement>()
                .HasOne(r => r.Category)
                .WithMany()
                .HasForeignKey(r => r.CategoryId)
                .OnDelete(DeleteBehavior.Restrict);

            // Configuración de la relación Requirement -> Priority
            modelBuilder.Entity<Requirement>()
                .HasOne(r => r.Priority)
                .WithMany()
                .HasForeignKey(r => r.PriorityId)
                .OnDelete(DeleteBehavior.SetNull);
            
            // Configuración de la relación muchos-a-muchos User -> Notification
            modelBuilder.Entity<UserNotification>()
                .HasKey(un => new { un.UserId, un.NotificationId });

            modelBuilder.Entity<UserNotification>()
                .HasOne(un => un.User)
                .WithMany(u => u.UserNotifications)
                .HasForeignKey(un => un.UserId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<UserNotification>()
                .HasOne(un => un.Notification)
                .WithMany(n => n.UserNotifications)
                .HasForeignKey(un => un.NotificationId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Comment>()
                .HasMany(c => c.Attachments) // Usamos HasMany() en lugar de SelectMany()
                .WithOne(a => a.Comment)
                .HasForeignKey(a => a.CommentId);

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
