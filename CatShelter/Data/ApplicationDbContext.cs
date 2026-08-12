using CatShelter.Models.Animal;
using CatShelter.Models.Blog;
using CatShelter.Models.Gallery;
using CatShelter.Models.Statistics;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace CatShelter.Data
{
    public class ApplicationDbContext:IdentityDbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext>options)
            : base(options) 
        {
        }

        public DbSet<Animal> Animals => Set<Animal>();
        public DbSet<Photo> Photos => Set<Photo>();
        public DbSet<Video> Videos => Set<Video>();
        public DbSet<Statistics> Statistics => Set<Statistics>();
        public DbSet<GalleryPhoto> GalleryPhotos => Set<GalleryPhoto>();
        public DbSet<BlogPost> BlogPosts => Set<BlogPost>();
        public DbSet<BlogBlock> BlogBlocks => Set<BlogBlock>();

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            builder.Entity<Animal>(entity =>
            {
                entity.Property(x => x.Name)
                    .IsRequired()
                    .HasMaxLength(100);

                entity.Property(x => x.ShortDescription)
                    .HasMaxLength(5000);

                entity.Property(x => x.Story)
                    .HasMaxLength(5000);

                entity.Property(x => x.Features)
                    .HasMaxLength(5000);

                entity.HasMany(x => x.Photos)
                    .WithOne(x => x.Animal)
                    .HasForeignKey(x => x.AnimalId)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasMany(x => x.Videos)
                    .WithOne(x => x.Animal)
                    .HasForeignKey(x => x.AnimalId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            builder.Entity<Photo>(entity =>
            {
                entity.Property(x => x.StorageKey)
                    .IsRequired()
                    .HasMaxLength(500);

                entity.Property(x => x.Comment)
                    .HasMaxLength(5000);                
            });

            builder.Entity<Video>(entity =>
            {
                entity.Property(x => x.Url)
                    .IsRequired()
                    .HasMaxLength(500);

                entity.Property(x => x.Comment)
                    .HasMaxLength(5000);                
            });

            builder.Entity<GalleryPhoto>(entity =>
            {
                entity.Property(x => x.StorageKey)
                    .IsRequired()
                    .HasMaxLength(500);

                entity.Property(x => x.Comment)
                    .HasMaxLength(5000);
            });

            builder.Entity<BlogPost>(entity =>
            {
                entity.Property(x => x.Title)
                    .IsRequired()
                    .HasMaxLength(200);

                entity.Property(x => x.Summary)
                    .HasMaxLength(1000);

                entity.HasMany(x => x.Blocks)
                    .WithOne(x => x.BlogPost)
                    .HasForeignKey(x => x.BlogPostId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            builder.Entity<BlogBlock>(entity =>
            {
                entity.Property(x => x.Text)
                    .HasMaxLength(10000);

                entity.Property(x => x.StorageKey)
                    .HasMaxLength(500);
            });

        }
    }
}
