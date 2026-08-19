using System.Diagnostics;
using CatShelter.Data;
using CatShelter.Models;
using CatShelter.ViewModels.Home;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using CatShelter.ViewModels.Statistics;
using CatShelter.ViewModels.Gallery;
using CatShelter.Services.PhotoStorage;
using CatShelter.Models.Animal;
using CatShelter.ViewModels.Animals;

namespace CatShelter.Controllers
{
    public class HomeController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IPhotoStorage _photoStorage;
        private readonly ILogger<HomeController> _logger;


        public HomeController(
            ApplicationDbContext context,
            IPhotoStorage photoStorage,
            ILogger<HomeController> logger)
        {
            _context = context;
            _photoStorage = photoStorage;
            _logger = logger;
        }
                

        public async Task<IActionResult> Index()
        {
            var statistics = await _context.Statistics
                .FirstOrDefaultAsync();

            var galleryPhotos = await _context.GalleryPhotos
                .OrderBy(x => x.SortOrder == null)
                .ThenBy(x => x.SortOrder)
                .ThenByDescending(x => x.CreatedAtUtc)
                .Take(8)
                .ToListAsync();

            var animals = await _context.Animals
                .Include(x => x.Photos)
                .Where(x => x.Status == Status.Available)
                .OrderBy(x => x.SortOrder == null)
                .ThenBy(x => x.SortOrder)
                .ThenByDescending(x => x.CreatedAtUtc)
                .Take(4)
                .ToListAsync();

            var animalCards = animals
                .Select(x =>
                {
                    var mainPhoto = x.Photos
                        .OrderByDescending(p => p.IsMain)
                        .ThenBy(p => p.SortOrder == null)
                        .ThenBy(p => p.SortOrder)
                        .FirstOrDefault();

                    return new AnimalCardViewModel
                    {
                        Id = x.Id,
                        Name = x.Name,
                        Sex = x.Sex == Sex.Male
                            ? "male"
                            : "femail",
                        Age = GetAgeText(x.Age),
                        IsSterilized = x.IsSterilized,
                        IsVaccinated = x.IsVaccinated,
                        MainPhotoUrl = mainPhoto is not null
                            ? _photoStorage.GetPublicUrl(mainPhoto.StorageKey)
                            : null
                    };
                    
                }).ToList();                       

            var model = new HomeViewModel
            {
                Statistics = new EditStatisticsViewModel
                {
                    CurrentAnimals = statistics?.CurrentAnimals ?? 0,
                    FoundHomes = statistics?.FoundHome ?? 0
                },

                GalleryPhotos = galleryPhotos
                    .Select(x => new GalleryPhotoViewModel
                    {
                        Id = x.Id,
                        Url = _photoStorage.GetPublicUrl(x.StorageKey),
                        Comment = x.Comment
                    })
                    .ToList(),

                Animals = animalCards                
            };

            return View(model);
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }

        private static string GetAgeText(Age? age)
        {
            if (age is null)
            {
                return "Unknown";
            }

            if (age.Years == 0)
            {
                return $"{age.Months} month";
            }

            if (age.Months == 0)
            {
                return $"{age.Years} years";
            }

            return $"{age.Years} years {age.Months} months";
        }
    }
}
