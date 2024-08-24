using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using pokemon.Data;
using pokemon.Models;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

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

            // Verificar si el equipo ya existe
            var equipoExistente = _context.Equipos
                .Include(e => e.Pokemons)
                .FirstOrDefault(e => e.UsuarioId == usuarioId && e.Nombre == model.NombreEquipo);

            if (equipoExistente != null)
            {
                return Json(new { success = false, message = "Ya existe un equipo con ese nombre." });
            }

            // Crear un nuevo equipo
            var equipo = new Equipo
            {
                Nombre = model.NombreEquipo,
                UsuarioId = usuarioId,
                Pokemons = model.Pokemons.Take(5).Select(p => new Pokemon
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
