namespace CatShelter.ViewModels.Animals
{
    public class AddPhotosViewModel
    {
        public int AnimalId { get; set; }

        public List<IFormFile> Files { get; set; } = [];
    }
}
