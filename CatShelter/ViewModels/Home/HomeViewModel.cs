using CatShelter.ViewModels.Animals;
using CatShelter.ViewModels.Gallery;
using CatShelter.ViewModels.Statistics;

namespace CatShelter.ViewModels.Home
{
    public class HomeViewModel
    {
        public EditStatisticsViewModel Statistics { get; set; } = new();

        public List<GalleryPhotoViewModel> GalleryPhotos { get; set; } = [];

        public List<AnimalCardViewModel> Animals { get; set; } = [];
    }
}
