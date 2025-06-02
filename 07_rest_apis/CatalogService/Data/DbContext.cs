using Microsoft.EntityFrameworkCore;
using CatalogService.Models;

namespace CatalogService.Data
{
    public class CatalogServiceDbContext : DbContext
    {
        public CatalogServiceDbContext(DbContextOptions<CatalogServiceDbContext> options) : base(options) { }

        public DbSet<Category> Categories { get; set; }
        public DbSet<Item> Items { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Category>();
            modelBuilder.Entity<Item>();
        }
    }
}