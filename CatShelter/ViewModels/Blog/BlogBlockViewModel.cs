using CatShelter.Models.Blog;

namespace CatShelter.ViewModels.Blog
{
    public class BlogBlockViewModel
    {
        public int Id { get; set; }

        public BlogBlockType Type { get; set; }

        public string? Text { get; set; }

        public string? Url { get; set; }

        public int SortOrder { get; set; }
    }
}
