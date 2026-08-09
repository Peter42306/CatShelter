namespace CatShelter.Models.Animal
{
    public class Photo
    {
        public int Id { get; set; }

        public int AnimalId { get; set; }

        public string Url { get; set; } = string.Empty;

        public string? Comment { get; set; }

        public bool IsMain { get; set; }

        public Animal Animal { get; set; } = null!;
    }
}
