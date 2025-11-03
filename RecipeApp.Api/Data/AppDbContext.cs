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
        public DbSet<UserFavorite> UserFavorites { get; set; } = null!;

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

                entity.HasOne(r => r.Category)
                      .WithMany()
                      .HasForeignKey(r => r.CategoryId)
                      .OnDelete(DeleteBehavior.SetNull);
            });

            // -----------------------
            // UserFavorite configuration
            // -----------------------
            modelBuilder.Entity<UserFavorite>(entity =>
            {
                entity.HasKey(f => f.Id);

                entity.HasOne(f => f.User)
                      .WithMany()
                      .HasForeignKey(f => f.UserId)
                      .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(f => f.Recipe)
                      .WithMany()
                      .HasForeignKey(f => f.RecipeId)
                      .OnDelete(DeleteBehavior.Cascade);

                // Prevent same user favoriting same recipe twice
                entity.HasIndex(f => new { f.UserId, f.RecipeId }).IsUnique();
            });
        }
    }
}