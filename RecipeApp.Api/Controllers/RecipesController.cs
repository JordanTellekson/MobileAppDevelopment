using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using RecipeApp.Api.Services;
using RecipeApp.Shared.Models;
using RecipeApp.Shared.Services;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace RecipeApp.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class RecipesController : ControllerBase
    {
        private readonly IRecipeService _recipeService;
        private readonly ILogger<RecipesController> _logger;

        public RecipesController(IRecipeService recipeService, ILogger<RecipesController> logger)
        {
            _recipeService = recipeService;
            _logger = logger;
        }

        // GET: api/recipes
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Recipe>>> GetAll()
        {
            await _recipeService.InitializeAsync(); // Reload data from database
            return Ok(_recipeService.Recipes);
        }

        // GET: api/recipes/{id}
        [HttpGet("{id:guid}")]
        public async Task<ActionResult<Recipe>> GetById(Guid id)
        {
            var recipe = await _recipeService.GetRecipeByIdAsync(id);
            if (recipe == null)
                return NotFound($"Recipe with id {id} not found.");

            return Ok(recipe);
        }

        // POST: api/recipes
        [HttpPost]
        public async Task<ActionResult<Recipe>> Create([FromBody] Recipe recipe)
        {
            if (recipe == null)
                return BadRequest("Recipe is null.");

            try
            {
                await _recipeService.AddRecipeAsync(recipe);
                await _recipeService.InitializeAsync(); // Reload to include new recipe
                return CreatedAtAction(nameof(GetById), new { id = recipe.Id }, recipe);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating recipe: {Title}", recipe.Title);
                return StatusCode(500, $"Internal server error: {ex.Message}\n{ex.StackTrace}");
            }
        }

        // PUT: api/recipes/{id}
        [HttpPut("{id:guid}")]
        public async Task<IActionResult> Update(Guid id, [FromBody] Recipe recipe)
        {
            if (recipe == null || id != recipe.Id)
                return BadRequest("Recipe is null or ID mismatch.");

            try
            {
                await _recipeService.UpdateRecipeAsync(recipe);
                await _recipeService.InitializeAsync(); // Reload to update collection
                return NoContent();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating recipe: {Title}", recipe.Title);
                return StatusCode(500, "Internal server error while updating recipe.");
            }
        }

        // DELETE: api/recipes/{id}
        [HttpDelete("{id:guid}")]
        public async Task<IActionResult> Delete(Guid id)
        {
            try
            {
                await _recipeService.DeleteRecipeAsync(id);
                await _recipeService.InitializeAsync(); // Reload to update collection
                return NoContent();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting recipe with ID {Id}", id);
                return StatusCode(500, "Internal server error while deleting recipe.");
            }
        }

        // POST: api/recipes/{id}/favorite
        [HttpPost("{id:guid}/favorite")]
        public async Task<IActionResult> AddToFavorites(Guid id)
        {
            var recipe = await _recipeService.GetRecipeByIdAsync(id);
            if (recipe == null) return NotFound($"Recipe with id {id} not found.");

            try
            {
                bool added = await _recipeService.AddToFavoritesAsync(recipe);
                if (!added)
                    return BadRequest("Recipe is already a favorite.");

                await _recipeService.InitializeAsync(); // Reload favorites
                return Ok(recipe);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error adding recipe to favorites: {Title}", recipe.Title);
                return StatusCode(500, "Internal server error while adding to favorites.");
            }
        }

        // DELETE: api/recipes/{id}/favorite
        [HttpDelete("{id:guid}/favorite")]
        public async Task<IActionResult> RemoveFromFavorites(Guid id)
        {
            var recipe = await _recipeService.GetRecipeByIdAsync(id);
            if (recipe == null) return NotFound($"Recipe with id {id} not found.");

            try
            {
                bool removed = await _recipeService.RemoveFromFavoritesAsync(recipe);
                if (!removed)
                    return BadRequest("Recipe was not in favorites.");

                await _recipeService.InitializeAsync(); // Reload favorites
                return NoContent();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error removing recipe from favorites: {Title}", recipe.Title);
                return StatusCode(500, "Internal server error while removing from favorites.");
            }
        }
    }
}