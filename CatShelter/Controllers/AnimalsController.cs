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

            var model = animals.Select(x => new AnimalsIndexViewModel
            {
                Id = x.Id,
                Name = x.Name,
                Sex = x.Sex,
                Age = x.Age,
                Status = x.Status,
                SortOrder = x.SortOrder
            }).ToList();

            return View(model);
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
                .Include(x => x.Videos)
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
                    .ToList(),

                Videos = animal.Videos
                    .OrderBy(x => x.SortOrder == null)
                    .ThenBy(x => x.SortOrder)
                    .ThenByDescending(x => x.CreatedAtUtc)
                    .Select(x => new VideoViewModel
                    {
                        Id = x.Id,
                        Url = x.Url,
                        EmbedUrl = GetYoutubeEmbedUrl(x.Url) ?? string.Empty,
                        Comment = x.Comment,
                        SortOrder= x.SortOrder,
                        CreatedAtUtc= x.CreatedAtUtc
                    }).ToList()
            };

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(
            EditAnimalViewModel model,
            CancellationToken ct)
        {
            ValidateBirthdate(model.BirthDate);

            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var animal = await _context.Animals.FindAsync(model.Id, ct);

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

            await _context.SaveChangesAsync(ct);

            return RedirectToAction(nameof(Index));
        }

        [HttpGet]
        public async Task<IActionResult> Delete(int id, CancellationToken ct)
        {
            var animal = await _context.Animals.FindAsync(id, ct);

            if (animal is null)
            {
                return NotFound();
            }

            return View(animal);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(
            int id,
            CancellationToken ct)
        {
            var animal = await _context.Animals
                .Include(x => x.Photos)
                .FirstOrDefaultAsync(x => x.Id == id, ct);

            if (animal is null)
            {
                return NotFound();
            }

            try
            {
                foreach (var photo in animal.Photos)
                {
                    await _photoStorage.DeleteAsync(photo.StorageKey, ct);
                }

                _context.Animals.Remove(animal);

                await _context.SaveChangesAsync(ct);


            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    "Failed to delete photos for animal {AnimalId}.",
                    animal.Id);

                throw;
            }            

            return RedirectToAction(nameof(Index));
        }

        [HttpPost]      
        [ValidateAntiForgeryToken]        
        public async Task<IActionResult> AddPhoto(
            AddPhotoViewModel model,
            CancellationToken ct)
        {
            const long maxFileSize = 10 * 1024 * 1024;

            if(model.File is null || model.File.Length == 0)
            {
                TempData["UploadError"] = "Select a photo.";
                return RedirectToAction(nameof(Edit), new { id = model.AnimalId });
            }

            if (model.File.Length > maxFileSize)
            {
                TempData["UploadError"] = $"File {model.File.FileName} exceeds {maxFileSize / (1024 * 1024)} MB.";
                return RedirectToAction(nameof(Edit), new { id = model.AnimalId });
            }

            var allowedExtensions = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
            {
                ".jpg",
                ".jpeg",
                ".png",
                ".webp"
            };

            var extension = Path.GetExtension(model.File.FileName);

            if (!allowedExtensions.Contains(extension))
            {
                TempData["UploadError"] = "Only JPG, PNG, and WebP photos are allowed.";
                return RedirectToAction(nameof(Edit), new { id = model.AnimalId });
            }

            var animalExists = await _context.Animals
                .AnyAsync(x => x.Id == model.AnimalId, ct);

            if (!animalExists)
            {
                return NotFound();
            }

            string? storageKey = null;

            try
            {
                storageKey = await _photoStorage.UploadAsync(
                    model.File,
                    $"animals/{model.AnimalId}",
                    ct);

                var photo = new Photo
                    {
                        AnimalId = model.AnimalId,
                        StorageKey = storageKey
                    };

                _context.Photos.Add(photo);                

                await _context.SaveChangesAsync(ct);
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    "Failed to upload photo for animal {AnimalId}. Starting storage cleanup.",
                    model.AnimalId);

                if (storageKey is not null)
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

            var currentMainPhoto = await _context.Photos
                .FirstOrDefaultAsync(
                    x => x.AnimalId == photo.AnimalId && x.IsMain,
                    ct);

            if (currentMainPhoto is not null)
            {
                currentMainPhoto.IsMain = false;
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

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddVideo(
            AddVideoViewModel model,
            CancellationToken ct)
        {
            var animalExists = await _context.Animals
                .AnyAsync(x => x.Id == model.AnimalId, ct);

            if (!animalExists)
            {
                return NotFound();
            }

            if (string.IsNullOrWhiteSpace(model.Url))
            {
                ModelState.AddModelError(
                    nameof(model.Url),
                    "Video URL is required.");
            }

            var videoId = GetYouTubeVideoId(model.Url);

            if (videoId is null)
            {
                ModelState.AddModelError(
                    nameof(model.Url),
                    "Enter a valid YouTube URL.");
            }

            if (!ModelState.IsValid)
            {
                return RedirectToAction(
                    nameof(Edit),
                    new { id = model.AnimalId });
            }

            var video = new Video
            {
                AnimalId = model.AnimalId,
                Url = model.Url,
                Comment = model.Comment,
                SortOrder = model.SortOrder
            };

            _context.Videos.Add(video);

            await _context.SaveChangesAsync(ct);

            return RedirectToAction(nameof(Edit), new { id = model.AnimalId });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditVideo(
            EditVideoViewModel model,
            CancellationToken ct)
        {
            var video = await _context.Videos
                .FirstOrDefaultAsync(x => x.Id == model.Id, ct);

            if (video is null)
            {
                return NotFound();
            }

            if (video.AnimalId != model.AnimalId)
            {
                return BadRequest();
            }

            var videoId = GetYouTubeVideoId(model.Url);

            if (videoId is null)
            {
                ModelState.AddModelError(
                    nameof(model.Url),
                    "Enter a valid YouTube URL.");

                return RedirectToAction(
                    nameof(Edit),
                    new { id = model.AnimalId });
            }

            video.Url = model.Url.Trim();
            video.Comment = model.Comment;
            video.SortOrder = model.SortOrder;

            await _context.SaveChangesAsync(ct);

            return RedirectToAction(
                nameof(Edit),
                new { id = video.AnimalId });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteVideo(
            int videoId,
            CancellationToken ct)
        {
            var video = await _context.Videos.FirstOrDefaultAsync(x => x.Id == videoId, ct);

            if (video is null)
            {
                return NotFound();
            }

            var animalId = video.AnimalId;

            _context.Videos.Remove(video);

            await _context.SaveChangesAsync(ct);

            return RedirectToAction(nameof(Edit), new { id = animalId });
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

        private static string? GetYouTubeVideoId(string url)
        {
            if (!Uri.TryCreate(url, UriKind.Absolute, out var uri))
            {
                return null;
            }

            if (uri.Host.Contains("youtu.be", StringComparison.OrdinalIgnoreCase))
            {
                return uri.AbsolutePath.Trim('/');
            }

            if (uri.Host.Contains("youtube.com", StringComparison.OrdinalIgnoreCase))
            {
                var query = Microsoft.AspNetCore.WebUtilities.QueryHelpers.ParseQuery(uri.Query);

                if (query.TryGetValue("v", out var videoId))
                {
                    return videoId.ToString();
                }
            }

            return null;
        }

        private static string? GetYoutubeEmbedUrl(string url)
        {
            var videoId = GetYouTubeVideoId(url);

            if (string.IsNullOrWhiteSpace(videoId))
            {
                return null;
            }

            return $"https://www.youtube.com/embed/{videoId}";
        }
    }
}
