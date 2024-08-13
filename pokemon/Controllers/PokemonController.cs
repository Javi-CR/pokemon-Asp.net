using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using pokemon.Data;
using pokemon.Models;
using System.Linq;
using System.Threading.Tasks;

namespace PokemonApp.Controllers
{
    [Authorize]
    public class PokemonController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<IdentityUser> _userManager;

        public PokemonController(ApplicationDbContext context, UserManager<IdentityUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }



        [HttpPost]
        public async Task<IActionResult> SelectPokemon(string name, string imageUrl, int life)
        {
            var userId = _userManager.GetUserId(User);

            // Verificar si el usuario ya tiene un Pokémon seleccionado
            var existingSelection = _context.PokemonSelections
                                             .FirstOrDefault(p => p.UserId == userId);

            if (existingSelection != null)
            {
                // Actualizar la selección existente
                existingSelection.Name = name;
                existingSelection.ImageUrl = imageUrl;
                existingSelection.Life = life;

                _context.PokemonSelections.Update(existingSelection);
            }
            else
            {
                // Crear una nueva selección si no existe
                var pokemonSelection = new PokemonSelection
                {
                    Name = name,
                    ImageUrl = imageUrl,
                    Life = life,
                    UserId = userId
                };

                _context.PokemonSelections.Add(pokemonSelection);
            }

            await _context.SaveChangesAsync();

            return RedirectToAction("Index", "Pokemon"); // Redirige a la vista de selección
        }
    }



}
