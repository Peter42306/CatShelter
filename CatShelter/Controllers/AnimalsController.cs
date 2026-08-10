using CatShelter.Data;
using CatShelter.Models.Animal;
using CatShelter.Services.PhotoStorage;
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
        private readonly IPhotoStorage _photoStorage;
        private readonly ILogger<AnimalsController> _logger;

        public AnimalsController(
            ApplicationDbContext context,
            IPhotoStorage photoStorage,
            ILogger<AnimalsController> logger)
        {
            _context = context;
            _photoStorage = photoStorage;
            _logger = logger;
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

            return RedirectToAction(nameof(Edit), new { id = animal.Id });
        }

        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            var animal = await _context.Animals
                .Include(x => x.Photos)
                .FirstOrDefaultAsync(x => x.Id == id);

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
                SortOrder = animal.SortOrder,

                Photos = animal.Photos
                    .OrderByDescending(x => x.IsMain)
                    .ThenBy(x => x.SortOrder == null)
                    .ThenBy(x => x.SortOrder)
                    .ThenByDescending(x => x.CreatedAtUtc)
                    .Select(x => new PhotoViewModel
                    {
                        Id = x.Id,
                        Url = _photoStorage.GetPublicUrl(x.StorageKey),
                        Comment = x.Comment,
                        IsMain = x.IsMain,
                        SortOrder = x.SortOrder,
                        CreatedAtUtc = x.CreatedAtUtc
                    })
                    .ToList()
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

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddPhotos(
            AddPhotosViewModel model,
            CancellationToken ct)
        {
            const int maxFiles = 10;
            const long maxFileSize = 10 * 1024 * 1024;

            var allowedExtensions = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
            {
                ".jpg",
                ".jpeg",
                ".png",
                ".webp"
            };

            var animalExists = await _context.Animals
                .AnyAsync(x => x.Id == model.AnimalId, ct);

            if (!animalExists)
            {
                return NotFound();
            }

            if (model.Files.Count == 0)
            {
                ModelState.AddModelError(
                    nameof(model.Files),
                    "Select at least one photo.");
            }

            if (model.Files.Count > maxFiles)
            {
                ModelState.AddModelError(
                    nameof(model.Files),
                    $"You can upload up to {maxFiles} photos per upload.");
            }

            foreach (var file in model.Files)
            {
                if (file.Length == 0)
                {
                    ModelState.AddModelError(
                        nameof(model.Files),
                        $"File {file.FileName} is empty.");

                    continue;
                }

                if (file.Length > maxFileSize)
                {
                    ModelState.AddModelError(
                        nameof(model.Files),
                        $"File {file.FileName} exceeds 10 MB.");
                }

                var extension = Path.GetExtension(file.FileName);

                if (!allowedExtensions.Contains(extension))
                {
                    ModelState.AddModelError(
                        nameof(model.Files),
                        $"File {file.FileName} has an unsupported format.");
                }
            }

            if (!ModelState.IsValid)
            {
                return RedirectToAction(nameof(Edit), new { id = model.AnimalId });
            }

            var uploadedKeys = new List<string>();

            try
            {
                foreach (var file in model.Files)
                {
                    var storageKey = await _photoStorage.UploadAsync(
                        file,
                        model.AnimalId,
                        ct);

                    uploadedKeys.Add(storageKey);                    

                    var photo = new Photo
                    {
                        AnimalId = model.AnimalId,
                        StorageKey = storageKey
                    };

                    _context.Photos.Add(photo);
                }

                await _context.SaveChangesAsync(ct);
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    "Failed to upload photos for animal {AnimalId}. Starting S3 cleanup.",
                    model.AnimalId);

                foreach (var storageKey in uploadedKeys)
                {
                    try
                    {
                        await _photoStorage.DeleteAsync(
                            storageKey,
                            CancellationToken.None);
                    }
                    catch (Exception cleanupEx)
                    {
                        _logger.LogError(
                            cleanupEx,
                            "Failed to delete S3 object {StorageKey} for animal {AnimalId}.",
                            storageKey,
                            model.AnimalId);
                    }
                    
                }

                throw;
            }

            

            return RedirectToAction(nameof(Edit), new { id = model.AnimalId });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SetMainPhoto(
            int photoId,
            CancellationToken ct)
        {
            var photo = await _context.Photos.FirstOrDefaultAsync(x => x.Id == photoId, ct);

            if (photo is null)
            {
                return NotFound();
            }

            var currentMainPhotos = await _context.Photos
                .FirstOrDefaultAsync(
                    x => x.AnimalId == photo.AnimalId && x.IsMain,
                    ct);

            if (currentMainPhotos is not null)
            {
                currentMainPhotos.IsMain = false;
            }

            photo.IsMain = true;

            await _context.SaveChangesAsync(ct);

            return RedirectToAction(nameof(Edit), new { id = photo.AnimalId });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeletePhoto(
            int photoId,
            CancellationToken ct)
        {
            var photo = await _context.Photos.FirstOrDefaultAsync(x => x.Id == photoId, ct);

            if (photo is null)
            {
                return NotFound();
            }

            var animalId = photo.AnimalId;

            try
            {
                await _photoStorage.DeleteAsync(photo.StorageKey, ct);

                _context.Photos.Remove(photo);

                await _context.SaveChangesAsync(ct);
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    "Failed to delete photo {PhotoId} for animal {AnimalId}.",
                    photo.Id,
                    photo.AnimalId);

                throw;
            }

            return RedirectToAction(nameof(Edit), new { id = animalId });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditPhoto(
            EditPhotoViewModel model,
            CancellationToken ct)
        {
            var photo = await _context.Photos.FirstOrDefaultAsync(x => x.Id == model.Id, ct);

            if (photo is null)
            {
                return NotFound();
            }

            if (photo.AnimalId != model.AnimalId)
            {
                return BadRequest();
            }

            photo.Comment = model.Comment;
            photo.SortOrder = model.SortOrder;

            await _context.SaveChangesAsync(ct);

            return RedirectToAction(nameof(Edit), new { id = photo.AnimalId });
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
