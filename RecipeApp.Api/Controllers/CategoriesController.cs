using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RecipeApp.Repositories;
using RecipeApp.Shared.Models;
using RecipeApp.Shared.Services;

namespace RecipeApp.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class CategoriesController : ControllerBase
    {
        private readonly IRecipeService _recipeService;

        public CategoriesController(IRecipeService recipeService)
        {
            _recipeService = recipeService;
        }

        // GET: api/categories
        [HttpGet]
        [AllowAnonymous]
        public async Task<ActionResult<IEnumerable<Category>>> GetAll()
        {
            await _recipeService.InitializeAsync();
            return Ok(_recipeService.Categories);
        }

        // GET: api/categories/{id}
        [HttpGet("{id}")]
        [AllowAnonymous]
        public async Task<ActionResult<Category>> GetById(Guid id)
        {
            var category = await _recipeService.GetCategoryByIdAsync(id);
            if (category == null) return NotFound();
            return Ok(category);
        }

        // POST: api/categories
        [HttpPost]
        [Authorize]
        public async Task<ActionResult<Category>> Create(Category category)
        {
            if (category == null || string.IsNullOrWhiteSpace(category.Name))
                return BadRequest("Category is null or missing a name.");

            await _recipeService.AddCategoryAsync(category);
            await _recipeService.InitializeAsync();

            return CreatedAtAction(nameof(GetById), new { id = category.Id }, category);
        }

        [HttpPut("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult> Update(Guid id, Category category)
        {
            if (id != category.Id)
                return BadRequest("Id mismatch.");

            var existing = await _recipeService.GetCategoryByIdAsync(id);
            if (existing == null) return NotFound();

            await _recipeService.UpdateCategoryAsync(category);
            return NoContent();
        }

        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult> Delete(Guid id)
        {
            var existing = await _recipeService.GetCategoryByIdAsync(id);
            if (existing == null) return NotFound();

            await _recipeService.DeleteCategoryAsync(id);
            return NoContent();
        }
    }
}