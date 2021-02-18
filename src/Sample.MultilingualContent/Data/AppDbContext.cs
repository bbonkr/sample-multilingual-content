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

            modelBuilder.Entity<Language>().Property(x => x.Id).ValueGeneratedOnAdd();
            modelBuilder.Entity<Post>().Property(x => x.Id).ValueGeneratedOnAdd();
            modelBuilder.Entity<Localization>().Property(x => x.Id).ValueGeneratedOnAdd();
            modelBuilder.Entity<LocalizationSet>().Property(x => x.Id).ValueGeneratedOnAdd();
        }
    }
}
