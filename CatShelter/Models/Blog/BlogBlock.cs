namespace CatShelter.Models.Blog
{
    public class BlogBlock
    {
        public int Id { get; set; }

        public int BlogPostId { get; set; }

        public BlogBlockType Type { get; set; }

        public string? Text { get; set; }

        public string? StorageKey { get; set; }

        public int SortOrder { get; set; }

        public BlogPost BlogPost { get; set; } = null!;
    }
}
