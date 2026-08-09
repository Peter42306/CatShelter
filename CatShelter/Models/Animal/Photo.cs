namespace CatShelter.Models.Animal
{
    public class Photo
    {
        public int Id { get; set; }

        public int AnimalId { get; set; }

        public string StorageKey { get; set; } = string.Empty;

        public string? Comment { get; set; }

        public bool IsMain { get; set; }

        public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;

        public int? SortOrder { get; set; }

        public Animal Animal { get; set; } = null!;
    }
}
