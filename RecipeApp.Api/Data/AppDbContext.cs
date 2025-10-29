using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using RecipeApp.Shared.Models;
using System.Linq;

namespace RecipeApp.Api.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options)
            : base(options)
        {
        }

        public DbSet<Recipe> Recipes { get; set; } = null!;
        public DbSet<Category> Categories { get; set; } = null!;
        public DbSet<User> Users { get; set; } = null!;

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // -----------------------
            // Category configuration
            // -----------------------
            modelBuilder.Entity<Category>(entity =>
            {
                entity.HasKey(c => c.Id);
                entity.Property(c => c.Name).IsRequired();
            });

            // -----------------------
            // Recipe configuration
            // -----------------------
            modelBuilder.Entity<Recipe>(entity =>
            {
                entity.HasKey(r => r.Id);
                entity.Property(r => r.Title).IsRequired();
                entity.Property(r => r.Instructions).HasColumnType("nvarchar(max)");

                // Convert Ingredients list to comma-separated string + add ValueComparer
                entity.Property(r => r.Ingredients)
                      .HasConversion(
                          v => string.Join(",", v),
                          v => v.Split(',', System.StringSplitOptions.RemoveEmptyEntries)
                                .Select(i => i.Trim())
                                .ToList()
                      )
                      .Metadata.SetValueComparer(
                          new ValueComparer<List<string>>(
                              (c1, c2) => c1.SequenceEqual(c2),
                              c => c.Aggregate(0, (a, v) => HashCode.Combine(a, v.GetHashCode())),
                              c => c.ToList()
                          )
                      );

                // -----------------------
                // Relationship with Category
                // -----------------------
                entity.HasOne(r => r.Category)           // Navigation property
                      .WithMany()                        // Category can have many Recipes
                      .HasForeignKey(r => r.CategoryId)  // FK in Recipe
                      .OnDelete(DeleteBehavior.SetNull); // Optional: if category deleted, Recipe.CategoryId = null
            });
        }
    }
}