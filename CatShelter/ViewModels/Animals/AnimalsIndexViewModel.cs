using CatShelter.Models.Animal;

namespace CatShelter.ViewModels.Animals
{
    public class AnimalsIndexViewModel
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public Sex Sex { get; set; }
        public Age? Age { get; set; }
        public Status Status { get; set; }
        public int? SortOrder { get; set; }
    }
}
