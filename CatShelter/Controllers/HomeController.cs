using System.Diagnostics;
using CatShelter.Data;
using CatShelter.Models;
using CatShelter.ViewModels.Home;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using CatShelter.ViewModels.Statistics;
using CatShelter.ViewModels.Gallery;
using CatShelter.Services.PhotoStorage;

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
                .Take(4)
                .ToListAsync();

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
                    .ToList()
                
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
    }
}
