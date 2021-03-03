using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using Microsoft.EntityFrameworkCore;

using Sample.MultilingualContent.Entities;

namespace Sample.MultilingualContent.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
        {

        }

        public DbSet<Post> Posts { get; set; }

        public DbSet<Language> Languages { get; set; }

        public DbSet<Localization> Localizations { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<Language>().HasKey(x => x.Id);
            modelBuilder.Entity<Language>().Property(x => x.Id).ValueGeneratedOnAdd();
            modelBuilder.Entity<Language>().Property(x => x.Code).HasMaxLength(40).IsRequired();
            modelBuilder.Entity<Language>().HasIndex(x => x.Code).IsUnique();

            modelBuilder.Entity<Post>().Property(x => x.Id).ValueGeneratedOnAdd();
            modelBuilder.Entity<Post>()
                .HasOne(x => x.Title)
                .WithOne()
                .HasForeignKey<Post>(x => x.TitleId)
                ;
            modelBuilder.Entity<Post>()
                .HasOne(x => x.Content)
                .WithOne()
                .HasForeignKey<Post>(x => x.ContentId)
                ;

            modelBuilder.Entity<LocalizationSet>().HasKey(x => x.Id);
            modelBuilder.Entity<LocalizationSet>().Property(x => x.Id).ValueGeneratedOnAdd();
            
            modelBuilder.Entity<LocalizationSet>()
                .HasMany(x => x.Contents)
                .WithOne(x => x.LocalizationSet)
                .HasForeignKey(x => x.Id);

            modelBuilder.Entity<Localization>().HasKey(x => new { x.Id, x.LanguageId });
            modelBuilder.Entity<Localization>().Property(x => x.Id).IsRequired();
            modelBuilder.Entity<Localization>().Property(x => x.LanguageId).IsRequired();
            modelBuilder.Entity<Localization>().HasOne(x => x.LocalizationSet).WithMany(x => x.Contents).HasForeignKey(x => x.Id);
            modelBuilder.Entity<Localization>().HasOne(x => x.Language).WithMany(x => x.Localizations).HasForeignKey(x => x.LanguageId);
        }
    }
}
