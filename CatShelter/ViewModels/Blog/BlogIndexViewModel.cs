namespace CatShelter.ViewModels.Blog
{
    public class BlogIndexViewModel
    {
        public int Id { get; set; }

        public string Title { get; set; } = string.Empty;

        public bool IsPublished { get; set; }

        public DateTime CreatedAtUtc { get; set; }

        public DateTime? PublishedAtUtc { get; set; }
    }
}
