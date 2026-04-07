using Microsoft.EntityFrameworkCore;
using ScientificActivityDatabaseImplement.Models;
using ScientificActivityDataModels.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;
using System.Text;
using System.Threading.Tasks;

namespace ScientificActivityDatabaseImplement
{
    public class ScientificActivityDatabase : DbContext
    {
        public ScientificActivityDatabase()
        {
        }

        public ScientificActivityDatabase(DbContextOptions<ScientificActivityDatabase> options)
            : base(options)
        {
        }

        public virtual DbSet<Researcher> Researchers { get; set; }
        public virtual DbSet<ResearcherInterest> ResearcherInterests { get; set; }
        public virtual DbSet<Publication> Publications { get; set; }
        public virtual DbSet<Journal> Journals { get; set; }
        public virtual DbSet<Conference> Conferences { get; set; }
        public virtual DbSet<Grant> Grants { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            if (!optionsBuilder.IsConfigured)
            {
                optionsBuilder.UseNpgsql(
                    "Host=localhost;Port=5432;Database=ScientificActivityDatabase;Username=postgres;Password=postgres");
            }

            base.OnConfiguring(optionsBuilder);
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<Researcher>(entity =>
            {
                entity.HasKey(x => x.Id);

                entity.HasIndex(x => x.Email).IsUnique();

                entity.Property(x => x.Email).IsRequired();
                entity.Property(x => x.PasswordHash).IsRequired();
                entity.Property(x => x.LastName).IsRequired();
                entity.Property(x => x.FirstName).IsRequired();
                entity.Property(x => x.Phone).IsRequired();
                entity.Property(x => x.Department).IsRequired();
                entity.Property(x => x.Position).IsRequired();

                entity.HasMany(x => x.Publications)
                    .WithOne(x => x.Researcher)
                    .HasForeignKey(x => x.ResearcherId)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasMany(x => x.Interests)
                    .WithOne(x => x.Researcher)
                    .HasForeignKey(x => x.ResearcherId)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasData(new Researcher
                {
                    Id = 1,
                    Email = "admin@science.local",
                    PasswordHash = "admin",
                    Role = UserRole.Администратор,
                    IsActive = true,
                    LastName = "Администратор",
                    FirstName = "Системы",
                    MiddleName = null,
                    Phone = "79990000000",
                    Department = "Администрация",
                    Position = "Администратор системы",
                    AcademicDegree = AcademicDegree.Не_указано,
                    ELibraryAuthorId = null,
                    ResearchTopics = "Администрирование системы"
                });
            });

            modelBuilder.Entity<ResearcherInterest>(entity =>
            {
                entity.HasKey(x => x.Id);

                entity.Property(x => x.Keyword).IsRequired();

                entity.Property(x => x.Weight)
                    .HasPrecision(10, 2);
            });

            modelBuilder.Entity<Publication>(entity =>
            {
                entity.HasKey(x => x.Id);

                entity.Property(x => x.Title).IsRequired();
                entity.Property(x => x.Authors).IsRequired();

                entity.HasOne(x => x.Researcher)
                    .WithMany(x => x.Publications)
                    .HasForeignKey(x => x.ResearcherId)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(x => x.Journal)
                    .WithMany(x => x.Publications)
                    .HasForeignKey(x => x.JournalId)
                    .OnDelete(DeleteBehavior.SetNull);

                entity.HasOne(x => x.Conference)
                    .WithMany(x => x.Publications)
                    .HasForeignKey(x => x.ConferenceId)
                    .OnDelete(DeleteBehavior.SetNull);
            });

            modelBuilder.Entity<Journal>(entity =>
            {
                entity.HasKey(x => x.Id);

                entity.Property(x => x.Title).IsRequired();

                entity.HasMany(x => x.Publications)
                    .WithOne(x => x.Journal)
                    .HasForeignKey(x => x.JournalId)
                    .OnDelete(DeleteBehavior.SetNull);
            });

            modelBuilder.Entity<Conference>(entity =>
            {
                entity.HasKey(x => x.Id);

                entity.Property(x => x.Title).IsRequired();

                entity.HasMany(x => x.Publications)
                    .WithOne(x => x.Conference)
                    .HasForeignKey(x => x.ConferenceId)
                    .OnDelete(DeleteBehavior.SetNull);
            });

            modelBuilder.Entity<Grant>(entity =>
            {
                entity.HasKey(x => x.Id);

                entity.Property(x => x.Title).IsRequired();
                entity.Property(x => x.Organization).IsRequired();

                entity.Property(x => x.Amount)
                    .HasPrecision(18, 2);
            });
        }
    }
}
