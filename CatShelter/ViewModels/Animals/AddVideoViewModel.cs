namespace CatShelter.ViewModels.Animals
{
    public class AddVideoViewModel
    {
        public int AnimalId { get; set; }

        public string Url { get; set; } = string.Empty;

        public string? Comment { get; set; }

        public int? SortOrder { get; set; }
    }
}
