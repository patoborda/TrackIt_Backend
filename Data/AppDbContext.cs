using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using trackit.server.Models;

namespace trackit.server.Data
{

        public class UserDbContext : IdentityDbContext<User> // User es tu clase que representa al usuario
        {
            public UserDbContext(DbContextOptions<UserDbContext> options) : base(options)
            {
            }

            public DbSet<Requirement> Requirements { get; set; }
            public DbSet<RequirementType> RequirementTypes { get; set; }
            public DbSet<Category> Categories { get; set; }
            public DbSet<RequirementRelation> RequirementRelations { get; set; }
            public DbSet<Priority> Priorities { get; set; } // Agrega esto
            public DbSet<RequirementActionLog> RequirementActionLogs { get; set; }
            public DbSet<InternalUser> InternalUsers { get; set; } = null!;
            public DbSet<ExternalUser> ExternalUsers { get; set; } = null!;
            public DbSet<AdminUser> AdminUsers { get; set; } = null!;

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


                // Configuración de la relación uno-a-uno entre User y AdminUser
                modelBuilder.Entity<AdminUser>()
                    .HasOne(admin => admin.User) // AdminUser tiene un User
                    .WithOne(user => user.AdminUser) // User tiene un AdminUser
                    .HasForeignKey<AdminUser>(admin => admin.Id); // La clave foránea está en AdminUser.Id

                // Configuración de la relación uno-a-uno entre User y InternalUser
                modelBuilder.Entity<InternalUser>()
                    .HasOne(internalUser => internalUser.User)
                    .WithOne(user => user.InternalUser)
                    .HasForeignKey<InternalUser>(internalUser => internalUser.Id);

                // Configuración de la relación uno-a-uno entre User y ExternalUser
                modelBuilder.Entity<ExternalUser>()
                    .HasOne(externalUser => externalUser.User)
                    .WithOne(user => user.ExternalUser)
                    .HasForeignKey<ExternalUser>(externalUser => externalUser.Id);


                
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
                // Seed Data para Priority
                modelBuilder.Entity<Priority>().HasData(
                    new Priority { Id = 4, TypePriority = "Urgente" },
                    new Priority { Id = 1, TypePriority = "Alta" },
                    new Priority { Id = 2, TypePriority = "Media" },
                    new Priority { Id = 3, TypePriority = "Baja" }
                );
            }

        }
}

