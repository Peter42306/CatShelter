using System.ComponentModel.DataAnnotations;

namespace CatShelter.ViewModels.Blog
{
    public class AddBlogPhotoViewModel
    {
        public int BlogPostId { get; set; }

        [Required]
        public IFormFile File { get; set; } = null!;
    }
}
