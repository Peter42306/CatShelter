using CatShelter.ViewModels.Editors;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace CatShelter.Controllers
{
    [Authorize(Roles = "Admin")]
    public class EditorsController : Controller
    {
        private readonly UserManager<IdentityUser> _userManager;

        public EditorsController(UserManager<IdentityUser> userManager)
        {
            _userManager = userManager;
        }

        
        // GET: /Editors
        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var editors = await _userManager.GetUsersInRoleAsync("Editor");

            return View(editors);
        }

        // GET: /Editors/Create
        [HttpGet]
        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(CreateEditorViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var editor = new IdentityUser
            {
                UserName = model.Email,
                Email = model.Email,
                EmailConfirmed = true
            };

            var result = await _userManager.CreateAsync(editor, model.Password);

            if (!result.Succeeded)
            {
                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError(string.Empty, error.Description);
                }

                return View(model);
            }

            var roleResult = await _userManager.AddToRoleAsync(editor, "Editor");

            if (!roleResult.Succeeded)
            {
                await _userManager.DeleteAsync(editor);

                foreach (var error in roleResult.Errors)
                {
                    ModelState.AddModelError(string.Empty, error.Description);
                }

                return View(model);
            }

            return RedirectToAction(nameof(Index));
        }        

        //GET: /Editors/Delete/{id}
        [HttpGet]
        public async Task<IActionResult> Delete(string id)
        {
            var editor = await _userManager.FindByIdAsync(id);

            if (editor is null)
            {
                return NotFound();
            }

            if (!await _userManager.IsInRoleAsync(editor, "Editor"))
            {
                return NotFound();
            }

            var model = new DeleteEditorViewModel
            {
                Id = editor.Id,
                Email = editor.Email ?? string.Empty
            };

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(string id)
        {
            var editor = await _userManager.FindByIdAsync(id);

            if (editor is null)
            {
                return NotFound();
            }

            if (!await _userManager.IsInRoleAsync(editor, "Editor"))
            {
                return NotFound();
            }

            var result = await _userManager.DeleteAsync(editor);

            if (!result.Succeeded)
            {
                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError(string.Empty, error.Description);
                }

                var model = new DeleteEditorViewModel
                {
                    Id = editor.Id,
                    Email = editor.Email ?? string.Empty
                };

                return View(nameof(Delete), model);                
            }

            return RedirectToAction(nameof(Index));
        }
    }
}
