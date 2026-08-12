using System.ComponentModel.DataAnnotations;

namespace CatShelter.ViewModels.Blog
{
    public class CreateBlogPostViewModel
    {
        [Required]
        [StringLength(200)]
        public string Title { get; set; } = string.Empty;
    }
}
