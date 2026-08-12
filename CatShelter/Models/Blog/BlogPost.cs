namespace CatShelter.Models.Blog
{
    public class BlogPost
    {
        public int Id { get; set; }

        public string Title { get; set; } = string.Empty;

        public string? Summary { get; set; }

        public bool IsPublished { get; set; }

        public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;

        public DateTime? PublishedAtUtc { get; set; }

        public ICollection<BlogBlock> Blocks { get; set; } = [];
    }
}
