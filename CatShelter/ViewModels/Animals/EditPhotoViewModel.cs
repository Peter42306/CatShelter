namespace CatShelter.ViewModels.Animals
{
    public class EditPhotoViewModel
    {
        public int Id { get; set; }

        public int AnimalId { get; set; }

        public string? Comment { get; set; }

        public int? SortOrder { get; set; }
    }
}
