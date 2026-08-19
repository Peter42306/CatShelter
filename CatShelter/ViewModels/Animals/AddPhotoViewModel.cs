namespace CatShelter.ViewModels.Animals
{
    public class AddPhotoViewModel
    {
        public int AnimalId { get; set; }

        public IFormFile? File { get; set; }
    }
}
