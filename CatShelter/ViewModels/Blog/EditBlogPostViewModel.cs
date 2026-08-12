using System.ComponentModel.DataAnnotations;

namespace CatShelter.ViewModels.Blog
{
    public class EditBlogPostViewModel
    {
        public int Id { get; set; }

        [Required]
        [StringLength(200)]
        public string Title { get; set; } = string.Empty;

        [StringLength(1000)]
        public string? Summary { get; set; }

        public bool IsPublished { get; set; }

        public DateTime? PublishedAtUtc { get; set; }

        public List<BlogBlockViewModel> Blocks { get; set; } = [];
    }
}
