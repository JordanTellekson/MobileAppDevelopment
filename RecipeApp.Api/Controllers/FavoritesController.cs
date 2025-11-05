using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using RecipeApp.Shared.Models;
using RecipeApp.Shared.Services;
using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;

namespace RecipeApp.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class FavoritesController : ControllerBase
    {
        private readonly IFavoriteService _favoriteService;
        private readonly IRecipeService _recipeService;
        private readonly ILogger<FavoritesController> _logger;

        public FavoritesController(
            IFavoriteService favoriteService,
            IRecipeService recipeService,
            ILogger<FavoritesController> logger)
        {
            _favoriteService = favoriteService;
            _recipeService = recipeService;
            _logger = logger;
        }

        // POST: api/favorites/{recipeId}
        [HttpPost("{recipeId:guid}")]
        public async Task<IActionResult> AddToFavorites(Guid recipeId)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (userId == null)
                return Unauthorized("User ID not found in token.");

            var recipe = await _recipeService.GetRecipeByIdAsync(recipeId);
            if (recipe == null)
                return NotFound($"Recipe with id {recipeId} not found.");

            try
            {
                bool added = await _favoriteService.AddFavoriteAsync(Guid.Parse(userId), recipeId);
                if (!added)
                    return BadRequest("Recipe is already a favorite.");

                return Ok(recipe);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error adding recipe {Id} to favorites for user {UserId}", recipeId, userId);
                return StatusCode(500, "Internal server error while adding favorite.");
            }
        }

        // DELETE: api/favorites/{recipeId}
        [HttpDelete("{recipeId:guid}")]
        public async Task<IActionResult> RemoveFromFavorites(Guid recipeId)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (userId == null)
                return Unauthorized("User ID not found in token.");

            try
            {
                bool removed = await _favoriteService.RemoveFavoriteAsync(Guid.Parse(userId), recipeId);
                if (!removed)
                    return BadRequest("Recipe was not in favorites.");

                return NoContent();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error removing recipe {Id} from favorites for user {UserId}", recipeId, userId);
                return StatusCode(500, "Internal server error while removing favorite.");
            }
        }

        // GET: api/favorites
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Recipe>>> GetFavorites()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (userId == null)
                return Unauthorized("User ID not found in token.");

            try
            {
                var favorites = await _favoriteService.GetUserFavoritesAsync(Guid.Parse(userId));
                return Ok(favorites);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting favorites for user {UserId}", userId);
                return StatusCode(500, "Internal server error while fetching favorites.");
            }
        }
    }
}