namespace CatShelter.ViewModels.Animals
{
    public class VideoViewModel
    {
        public int Id { get; set; }

        public string Url { get; set; } = string.Empty;

        public string EmbedUrl { get; set; } = string.Empty;

        public string? Comment { get; set; }

        public int? SortOrder { get; set; }

        public DateTime CreatedAtUtc { get; set; }
    }
}
