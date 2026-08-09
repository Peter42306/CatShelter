namespace CatShelter.Models.Animal
{
    public class Video
    {
        public int Id { get; set; }

        public int AnimalId { get; set; }

        public string Url { get; set; } = string.Empty;

        public string? Comment { get; set; }

        public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;

        public int? SortOrder { get; set; }

        public Animal Animal { get; set; } = null!;
    }
}
