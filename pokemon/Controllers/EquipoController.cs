using Microsoft.AspNetCore.Mvc;
using pokemon.Data;
using pokemon.Models;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using pokemon.Data; // Asegúrate de usar el namespace correcto
using pokemon.Models; // Asegúrate de usar el namespace correcto

public class EquipoController : Controller
{
    private readonly ApplicationDbContext _context;

    public EquipoController(ApplicationDbContext context)
    {
        _context = context;
    }

    [HttpPost]
    public async Task<IActionResult> Crear([FromBody] EquipoViewModel model)
    {
        if (ModelState.IsValid)
        {
            var usuarioId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var equipo = new Equipo
            {
                Nombre = model.NombreEquipo,
                UsuarioId = usuarioId,
                Pokemons = model.Pokemons.Select(p => new Pokemon
                {
                    Nombre = p.Name,
                    ImagenUrl = p.ImageUrl,
                    Vida = p.Life
                }).ToList()
            };

            _context.Equipos.Add(equipo);
            await _context.SaveChangesAsync();

            return Json(new { success = true });
        }

        return Json(new { success = false });
    }
}
