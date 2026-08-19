using CatShelter.Data;
using CatShelter.Models.Gallery;
using CatShelter.Services.PhotoStorage;
using CatShelter.ViewModels.Gallery;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CatShelter.Controllers
{
    [Authorize(Roles = "Admin,Editor")]
    public class GalleryController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IPhotoStorage _photoStorage;
        private readonly ILogger<GalleryController> _logger;

        public GalleryController(
            ApplicationDbContext context,
            IPhotoStorage photoStorage,
            ILogger<GalleryController> logger)
        {
            _context = context;
            _photoStorage = photoStorage;
            _logger = logger;
        }

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var photos = await _context.GalleryPhotos
                .OrderBy(x => x.SortOrder == null)
                .ThenBy(x => x.SortOrder)
                .ThenByDescending(x => x.CreatedAtUtc)
                .ToListAsync();

            var model = photos
                .Select(x => new GalleryPhotoViewModel
                {
                    Id = x.Id,
                    Url = _photoStorage.GetPublicUrl(x.StorageKey),
                    Comment = x.Comment,
                    SortOrder = x.SortOrder
                })
                .ToList();

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddPhoto(
            AddGalleryPhotoViewModel model,
            CancellationToken ct)
        {
            const long maxFileSize = 10 * 1024 * 1024;

            var allowedExtensions = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
            {
                ".jpg",
                ".jpeg",
                ".png",
                ".webp"
            };

            if (model.File is null || model.File.Length == 0)
            {
                TempData["UploadError"] = "Select a photo.";
                return RedirectToAction(nameof(Index));
            }

            if (model.File.Length > maxFileSize)
            {
                TempData["UploadError"] = $"File {model.File.FileName} exceeds 10 MB.";
                return RedirectToAction(nameof(Index));
            }

            var extension = Path.GetExtension(model.File.FileName);

            if (!allowedExtensions.Contains(extension))
            {
                TempData["UploadError"] = "Only JPG, PNG, and WebP photos are allowed.";
                return RedirectToAction(nameof(Index));
            }

            string? storageKey = null;

            try
            {
                storageKey = await _photoStorage.UploadAsync(
                    model.File,
                    "gallery",
                    ct);

                var galleryPhoto = new GalleryPhoto
                {
                    StorageKey = storageKey
                };

                _context.GalleryPhotos.Add(galleryPhoto);

                await _context.SaveChangesAsync(ct);
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    "Failed to upload gallery photo. Starting storage cleanup.");

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
                            "Failed to delete gallery S3 object {StorageKey}.",
                            storageKey);
                    }
                }

                throw;
            }

            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditPhoto(
            EditGalleryPhotoViewModel model,
            CancellationToken ct)
        {
            var photo = await _context.GalleryPhotos
                .FirstOrDefaultAsync(x => x.Id == model.Id, ct);

            if (photo is null)
            {
                return NotFound();
            }

            photo.Comment = model.Comment;
            photo.SortOrder = model.SortOrder;

            await _context.SaveChangesAsync(ct);

            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeletePhoto(
            int photoId,
            CancellationToken ct)
        {
            var photo = await _context.GalleryPhotos
                .FirstOrDefaultAsync(x => x.Id == photoId, ct);

            if (photo is null)
            {
                return NotFound();
            }

            try
            {
                await _photoStorage.DeleteAsync(
                    photo.StorageKey,
                    ct);

                _context.GalleryPhotos.Remove(photo);

                await _context.SaveChangesAsync(ct);
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    "Failed to delete gallery photo {PhotoId}.",
                    photo.Id);

                throw;
            }

            return RedirectToAction(nameof(Index));
        }
    }
}
