namespace CatShelter.ViewModels.Gallery
{
    public class GalleryPhotoViewModel
    {
        public int Id { get; set; }

        public string Url { get; set; } = string.Empty;

        public string? Comment { get; set; }

        public int? SortOrder { get; set; }
    }
}
