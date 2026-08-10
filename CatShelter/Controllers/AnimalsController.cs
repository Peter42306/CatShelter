using CatShelter.Data;
using CatShelter.Models.Animal;
using CatShelter.ViewModels.Animals;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CatShelter.Controllers
{
    [Authorize(Roles = "Admin,Editor")]
    public class AnimalsController : Controller
    {
        private readonly ApplicationDbContext _context;

        public AnimalsController(ApplicationDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var animals = await _context.Animals
                .OrderBy(x => x.SortOrder == null)
                .ThenBy(x => x.SortOrder)
                .ThenByDescending(x => x.CreatedAtUtc)
                .ToListAsync();

            return View(animals);
        }

        [HttpGet]
        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(CreateAnimalViewModel model)
        {
            ValidateBirthdate(model.BirthDate);

            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var birthDate = model.BirthDate;

            if (model.IsBirthDateApproximate && birthDate is not null)
            {
                birthDate = new DateOnly(
                    birthDate.Value.Year,
                    birthDate.Value.Month,
                    1);
            }

            var animal = new Animal
            {
                Name = model.Name,
                Sex = model.Sex,
                BirthDate = birthDate,
                IsBirthDateApproximate = model.IsBirthDateApproximate,
                IsSterilized = model.IsSterilized,
                IsVaccinated = model.IsVaccinated,
                ShortDescription = model.ShortDescription,
                Story = model.Story,
                Features = model.Features,
                Status = model.Status,
                SortOrder = model.SortOrder
            };

            _context.Animals.Add(animal);

            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }

        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            var animal = await _context.Animals.FindAsync(id);

            if (animal is null)
            {
                return NotFound();
            }




            var model = new EditAnimalViewModel
            {
                Id = animal.Id,
                Name = animal.Name,
                Sex = animal.Sex,
                BirthDate = animal.BirthDate,
                IsBirthDateApproximate=animal.IsBirthDateApproximate,
                IsSterilized = animal.IsSterilized,
                IsVaccinated = animal.IsVaccinated,
                ShortDescription = animal.ShortDescription,
                Story = animal.Story,
                Features = animal.Features,
                Status = animal.Status,
                SortOrder = animal.SortOrder
            };

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(EditAnimalViewModel model)
        {
            ValidateBirthdate(model.BirthDate);

            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var animal = await _context.Animals.FindAsync(model.Id);

            if (animal is null)
            {
                return NotFound();
            }

            var birthDate = model.BirthDate;

            if (model.IsBirthDateApproximate && birthDate is not null)
            {
                birthDate = new DateOnly(
                    birthDate.Value.Year,
                    birthDate.Value.Month,
                    1);
            }

            animal.Name = model.Name;
            animal.Sex = model.Sex;
            animal.BirthDate = birthDate;
            animal.IsBirthDateApproximate = model.IsBirthDateApproximate;
            animal.IsSterilized = model.IsSterilized;
            animal.IsVaccinated = model.IsVaccinated;
            animal.ShortDescription = model.ShortDescription;
            animal.Story = model.Story;
            animal.Features = model.Features;
            animal.SortOrder = model.SortOrder;
            animal.Status = model.Status;            

            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }

        [HttpGet]
        public async Task<IActionResult> Delete(int id)
        {
            var animal = await _context.Animals.FindAsync(id);

            if (animal is null)
            {
                return NotFound();
            }

            return View(animal);
        }

        [HttpPost]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var animal = await _context.Animals.FindAsync(id);

            if (animal is null)
            {
                return NotFound();
            }

            _context.Animals.Remove(animal);

            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }

        // Helpers
        private void ValidateBirthdate(DateOnly? birthDate)
        {
            if (birthDate is not null && 
                birthDate > DateOnly.FromDateTime(DateTime.Today))
            {
                ModelState.AddModelError(
                    "BirthDate", "Birth date cannot be in the future.");
            }
        }
    }
}
