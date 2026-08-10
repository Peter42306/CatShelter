using CatShelter.Models.Animal;

namespace CatShelter.ViewModels.Animals
{
    public class PhotoViewModel
    {
        public int Id { get; set; }

        public string Url { get; set; } = string.Empty;

        public string? Comment { get; set; }

        public bool IsMain { get; set; }

        public DateTime CreatedAtUtc { get; set; }

        public int? SortOrder { get; set; }        
    }
}
