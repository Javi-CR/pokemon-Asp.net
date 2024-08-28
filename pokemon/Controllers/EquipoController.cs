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

            // Verificar si ya existe un equipo para el usuario
            var equipoExistente = await _context.Equipos
                .Include(e => e.Pokemons)
                .FirstOrDefaultAsync(e => e.UsuarioId == usuarioId);

            if (equipoExistente != null)
            {
                // Si ya existe un equipo, actualizar el existente
                equipoExistente.Nombre = model.NombreEquipo;
                equipoExistente.Pokemons.Clear();

                equipoExistente.Pokemons = model.Pokemons.Take(5).Select(p => new Pokemon
                {
                    Nombre = p.Name,
                    ImagenUrl = p.ImageUrl,
                    Vida = p.Life
                }).ToList();

                _context.Equipos.Update(equipoExistente);
            }
            else
            {
                // Si no existe un equipo, crear uno nuevo
                var nuevoEquipo = new Equipo
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

                _context.Equipos.Add(nuevoEquipo);
            }

            await _context.SaveChangesAsync();

            return Json(new { success = true });
        }

        return Json(new { success = false });
    }
}
