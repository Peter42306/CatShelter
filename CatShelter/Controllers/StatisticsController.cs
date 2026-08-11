using CatShelter.Data;
using CatShelter.ViewModels.Statistics;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using CatShelter.Models.Statistics;

namespace CatShelter.Controllers
{
    [Authorize(Roles = "Admin,Editor")]
    public class StatisticsController : Controller
    {
        private readonly ApplicationDbContext _context;

        public StatisticsController(ApplicationDbContext context)
        {
            _context = context;
        }

        [HttpGet]        
        public async Task<IActionResult> Edit()
        {
            var statistics = await _context.Statistics.FirstOrDefaultAsync();

            var model = statistics is null
                ? new EditStatisticsViewModel()
                : new EditStatisticsViewModel
                {
                    CurrentAnimals = statistics.CurrentAnimals,
                    FoundHomes = statistics.FoundHome
                };

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(
            EditStatisticsViewModel model,
            CancellationToken ct)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var statistics = await _context.Statistics.FirstOrDefaultAsync();

            if (statistics is null)
            {
                statistics = new Statistics();

                _context.Statistics.Add(statistics);
            }

            statistics.CurrentAnimals = model.CurrentAnimals;
            statistics.FoundHome = model.FoundHomes;

            await _context.SaveChangesAsync(ct);

            return RedirectToAction(nameof(HomeController.Index), "Home", fragment: "stats");
        }
    }
}
