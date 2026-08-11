using System.Diagnostics;
using CatShelter.Data;
using CatShelter.Models;
using CatShelter.ViewModels.Home;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using CatShelter.ViewModels.Statistics;

namespace CatShelter.Controllers
{
    public class HomeController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<HomeController> _logger;


        public HomeController(
            ApplicationDbContext context,
            ILogger<HomeController> logger)
        {
            _context = context;
            _logger = logger;
        }
                

        public async Task<IActionResult> Index()
        {
            var statistics = await _context.Statistics.FirstOrDefaultAsync();

            var model = new HomeViewModel
            {
                Statistics = new EditStatisticsViewModel
                {
                    CurrentAnimals = statistics?.CurrentAnimals ?? 0,
                    FoundHomes = statistics?.FoundHome ?? 0
                }
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
