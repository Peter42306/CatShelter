namespace CatShelter.Models.Gallery
{
    public class GalleryPhoto
    {
        public int Id { get; set; }

        public string StorageKey { get; set; } = string.Empty;

        public string? Comment { get; set; }

        public int? SortOrder { get; set; }

        public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;
    }
}
