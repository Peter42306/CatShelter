using CatShelter.Data;
using CatShelter.Models.Blog;
using CatShelter.Services.PhotoStorage;
using CatShelter.ViewModels.Blog;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CatShelter.Controllers
{
    [Authorize(Roles = "Admin,Editor")]
    public class BlogController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IPhotoStorage _photoStorage;
        private readonly ILogger<BlogController> _logger;

        public BlogController(
            ApplicationDbContext context,
            IPhotoStorage photoStorage,
            ILogger<BlogController> logger)
        {
            _context = context;
            _photoStorage = photoStorage;
            _logger=logger;
        }

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var blogPosts = await _context.BlogPosts
                .OrderByDescending(x => x.Id)
                .ToListAsync();

            var model = blogPosts
                .Select(x => new BlogIndexViewModel
                {
                    Id = x.Id,
                    Title = x.Title,
                    IsPublished = x.IsPublished,
                    CreatedAtUtc = x.CreatedAtUtc,
                    PublishedAtUtc = x.PublishedAtUtc
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
        public async Task<IActionResult> Create(
            CreateBlogPostViewModel model,
            CancellationToken ct)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var blogPost = new BlogPost
            {
                Title = model.Title                
            };

            _context.BlogPosts.Add(blogPost);

            await _context.SaveChangesAsync(ct);

            return RedirectToAction(nameof(Edit), new { id = blogPost.Id });
        }

        [HttpGet]
        public async Task<IActionResult> Edit(
            int id,
            CancellationToken ct)
        {
            var blogPost = await _context.BlogPosts
                .Include(x => x.Blocks)
                .FirstOrDefaultAsync(x => x.Id == id, ct);

            if (blogPost is null)
            {
                return NotFound();
            }

            var model = new EditBlogPostViewModel
            {
                Id = blogPost.Id,
                Title = blogPost.Title,
                Summary = blogPost.Summary,
                IsPublished = blogPost.IsPublished,
                PublishedAtUtc = blogPost.PublishedAtUtc,

                Blocks = blogPost.Blocks
                    .OrderBy(x => x.SortOrder)                    
                    .Select(x => new BlogBlockViewModel
                    {
                        Id = x.Id,
                        Type = x.Type,
                        Text = x.Text,

                        Url = x.Type == BlogBlockType.Photo && !string.IsNullOrWhiteSpace(x.StorageKey)
                            ? _photoStorage.GetPublicUrl(x.StorageKey) 
                            : null,

                        SortOrder = x.SortOrder
                    })
                    .ToList()
            };

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(
            EditBlogPostViewModel model,
            CancellationToken ct)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var blogPost = await _context.BlogPosts.FirstOrDefaultAsync(x => x.Id == model.Id, ct);

            if (blogPost is null)
            {
                return NotFound();
            }

            blogPost.Title = model.Title;
            blogPost.Summary = model.Summary;

            if (!blogPost.IsPublished && model.IsPublished)
            {
                blogPost.PublishedAtUtc = DateTime.UtcNow;
            }

            if (blogPost.IsPublished && !model.IsPublished)
            {
                blogPost.PublishedAtUtc = null;
            }

            blogPost.IsPublished = model.IsPublished;

            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }

        [HttpGet]
        public async Task<IActionResult> Delete(
            int id, 
            CancellationToken ct)
        {
            var blogPost = await _context.BlogPosts.FindAsync(id, ct);

            if (blogPost is null)
            {
                return NotFound();
            }

            return View(blogPost);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(
            int id,
            CancellationToken ct)
        {
            var blogPost = await _context.BlogPosts
                .Include(x => x.Blocks)
                .FirstOrDefaultAsync(x => x.Id == id, ct);

            if (blogPost is null)
            {
                return NotFound();
            }

            try
            {
                foreach (var block in blogPost.Blocks)
                {
                    if (block.Type == BlogBlockType.Photo && !string.IsNullOrWhiteSpace(block.StorageKey))
                    {
                        await _photoStorage.DeleteAsync(block.StorageKey, ct);
                    }
                }

                _context.BlogPosts.Remove(blogPost);
                
                await _context.SaveChangesAsync(ct);
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    "Failed to delete blog photos for blog post {BlogPostId}.",
                    blogPost.Id);

                throw;
            }            

            return RedirectToAction(nameof(Index));
        }



        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddParagraph(
            AddBlogParagraphViewModel model,
            CancellationToken ct)
        {
            if (!ModelState.IsValid)
            {
                return RedirectToAction(nameof(Edit), new { id = model.BlogPostId });
            }

            var blogPostExists = await _context.BlogPosts.AnyAsync(x => x.Id == model.BlogPostId);

            if (!blogPostExists)
            {
                return NotFound();
            }

            var maxSortOrder = await _context.BlogBlocks
                .Where(x => x.BlogPostId == model.BlogPostId)
                .Select(x => (int?)x.SortOrder)
                .MaxAsync(ct) ?? 0;

            var block = new BlogBlock
            {
                BlogPostId = model.BlogPostId,
                Type = BlogBlockType.Paragraph,
                Text = model.Text,
                SortOrder = maxSortOrder + 1
            };

            _context.BlogBlocks.Add(block);

            await _context.SaveChangesAsync(ct);

            return RedirectToAction(nameof(Edit), new { id = model.BlogPostId });
        }

        [HttpGet]
        public async Task<IActionResult> EditParagraph(
            int id,
            CancellationToken ct)
        {
            var block = await _context.BlogBlocks
                .FirstOrDefaultAsync(x => 
                    x.Id == id &&
                    x.Type == BlogBlockType.Paragraph);

            if (block is null)
            {
                return NotFound();
            }

            var model = new EditBlogParagraphViewModel
            {
                Id = block.Id,
                BlogPostId = block.BlogPostId,
                Text = block.Text ?? string.Empty
            };

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditParagraph(
            EditBlogParagraphViewModel model,
            CancellationToken ct)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var block = await _context.BlogBlocks
                .FirstOrDefaultAsync(x => 
                    x.Id == model.Id &&
                    x.Type == BlogBlockType.Paragraph,
                    ct);

            if (block is null)
            {
                return NotFound();
            }

            block.Text = model.Text;

            await _context.SaveChangesAsync(ct);

            return RedirectToAction(nameof(Edit), new { id = block.BlogPostId });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteBlock(
            int id,
            CancellationToken ct)
        {
            var block = await _context.BlogBlocks
                .FirstOrDefaultAsync(x => x.Id == id, ct);

            if (block is null)
            {
                return NotFound();
            }

            var blogPostId = block.BlogPostId;

            if (block.Type == BlogBlockType.Photo && !string.IsNullOrWhiteSpace(block.StorageKey))
            {
                try
                {
                    await _photoStorage.DeleteAsync(block.StorageKey, ct);
                }
                catch (Exception ex)
                {
                    _logger.LogError(
                        ex,
                        "Failed to delete blog photo {StorageKey}.",
                        block.StorageKey);

                    return RedirectToAction(nameof(Edit), new { id = blogPostId });
                }                
            }

            _context.BlogBlocks.Remove(block);

            await _context.SaveChangesAsync(ct);

            return RedirectToAction(nameof(Edit), new { id = blogPostId });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddPhoto(
            AddBlogPhotoViewModel model,
            CancellationToken ct)
        {
            const long maxFileSize = 10 * 1024 * 1024;

            var allowedExtensions = new HashSet<string>(
                StringComparer.OrdinalIgnoreCase)
            {
                ".jpg",
                ".jpeg",
                ".png",
                ".webp"
            };

            if (model.File is null || model.File.Length == 0)
            {
                return RedirectToAction(nameof(Edit), new { id = model.BlogPostId });
            }

            if (model.File.Length > maxFileSize)
            {
                return RedirectToAction(
                    nameof(Edit),
                    new { id = model.BlogPostId });
            }

            var extension = Path.GetExtension(model.File.FileName);

            if (!allowedExtensions.Contains(extension))
            {
                return RedirectToAction(
                    nameof(Edit),
                    new { id = model.BlogPostId });
            }

            var blogPostExists = await _context.BlogPosts
                .AnyAsync(x => x.Id == model.BlogPostId, ct);

            if (!blogPostExists)
            {
                return NotFound();
            }

            var maxSortOrder = await _context.BlogBlocks
                .Where(x => x.BlogPostId == model.BlogPostId)
                .Select(x => (int?)x.SortOrder)
                .MaxAsync(ct) ?? 0;

            string? storageKey = null;

            try
            {
                storageKey = await _photoStorage.UploadAsync(
                    model.File,
                    $"blog/{model.BlogPostId}",
                    ct);

                var block = new BlogBlock
                {
                    BlogPostId = model.BlogPostId,
                    Type = BlogBlockType.Photo,
                    StorageKey = storageKey,
                    SortOrder = maxSortOrder + 1
                };

                _context.BlogBlocks.Add(block);

                await _context.SaveChangesAsync(ct);
            }
            catch
            {
                if (storageKey is not null)
                {
                    await _photoStorage.DeleteAsync(
                        storageKey,
                        CancellationToken.None);
                }

                throw;
            }

            return RedirectToAction(nameof(Edit), new { id = model.BlogPostId });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> MoveBlockUp(
            int id,
            CancellationToken ct)
        {
            var block = await _context.BlogBlocks.FirstOrDefaultAsync(x => x.Id == id, ct);

            if (block is null)
            {
                return NotFound();
            }

            var previousBlock = await _context.BlogBlocks
                .Where(x => 
                    x.BlogPostId == block.BlogPostId &&
                    x.SortOrder < block.SortOrder)
                .OrderByDescending(x => x.SortOrder)
                .FirstOrDefaultAsync(ct);

            if (previousBlock is not null)
            {
                var currentSortOrder = block.SortOrder;

                block.SortOrder = previousBlock.SortOrder;
                previousBlock.SortOrder = currentSortOrder;

                await _context.SaveChangesAsync(ct);
            }

            return RedirectToAction(nameof(Edit), new { id = block.BlogPostId });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> MoveBlockDown(
            int id,
            CancellationToken ct)
        {
            var block = await _context.BlogBlocks.FirstOrDefaultAsync(x => x.Id == id, ct);

            if (block is null)
            {
                return NotFound();
            }

            var nextBlock = await _context.BlogBlocks
                .Where(x =>
                    x.BlogPostId == block.BlogPostId &&
                    x.SortOrder > block.SortOrder)
                .OrderBy(x => x.SortOrder)
                .FirstOrDefaultAsync(ct);

            if (nextBlock is not null)
            {
                var currentSortOrder = block.SortOrder;

                block.SortOrder = nextBlock.SortOrder;
                nextBlock.SortOrder = currentSortOrder;

                await _context.SaveChangesAsync(ct);
            }

            return RedirectToAction(nameof(Edit), new { id = block.BlogPostId });
        }
    }
}
