namespace CatShelter.ViewModels.Animals
{
    public class AnimalCardViewModel
    {
        public int Id { get; set; }

        public string Name { get; set; } = string.Empty;

        public string Sex { get; set; } = string.Empty;

        public string Age { get; set; } = string.Empty;

        public bool IsSterilized { get; set; }

        public bool IsVaccinated { get; set; }

        public string? MainPhotoUrl { get; set; }
    }
}