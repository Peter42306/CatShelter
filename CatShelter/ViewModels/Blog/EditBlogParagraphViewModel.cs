using System.ComponentModel.DataAnnotations;

namespace CatShelter.ViewModels.Blog
{
    public class EditBlogParagraphViewModel
    {
        public int Id { get; set; }

        public int BlogPostId { get; set; }

        [Required]
        [StringLength(10000)]
        public string Text { get; set; } = string.Empty;
    }
}
