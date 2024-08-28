using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json.Linq;
using pokemon.Data;
using pokemon.Models;
using System.Diagnostics;
using System.Security.Claims;

namespace pokemon.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly ApplicationDbContext _context;
        private readonly UserManager<IdentityUser> _userManager;

        public HomeController(ApplicationDbContext context, UserManager<IdentityUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        public IActionResult Index()
        {
            return View();
        }

        [Route("Pokedex")]
        public IActionResult Pokedex()
        {
            return View();
        }

        [Route("TuEquipo")]
        [Authorize(Roles = "entrenador")]
        public IActionResult TuEquipo()
        {
            return View();
        }

        [Route("Pokemon")]
        [Authorize(Roles = "entrenador")]
        public IActionResult Pokemon()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Retar(string retadoId)
        {
            var usuarioId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            var equipoUsuario = await _context.Equipos
                .Include(e => e.Pokemons)
                .FirstOrDefaultAsync(e => e.UsuarioId == usuarioId);

            var equipoRetado = await _context.Equipos
                .Include(e => e.Pokemons)
                .FirstOrDefaultAsync(e => e.UsuarioId == retadoId);

            if (equipoUsuario == null || equipoRetado == null)
            {
                return BadRequest("Uno de los equipos no existe.");
            }

            var random = new Random();
            var ganador = random.Next(2) == 0 ? equipoUsuario : equipoRetado;
            var perdedor = ganador == equipoUsuario ? equipoRetado : equipoUsuario;

            foreach (var pokemon in perdedor.Pokemons)
            {
                pokemon.Vida = 0;
            }

            await _context.SaveChangesAsync();

            TempData["Resultado"] = ganador == equipoUsuario
                ? $"¡Ganaste el reto contra {equipoRetado.Nombre}!"
                : $"Perdiste el reto contra {equipoRetado.Nombre}. Todos tus Pokémon están debilitados.";

            return RedirectToAction("Retos");
        }


        [Route("Retos")]
        [Authorize(Roles = "entrenador")]
        public async Task<IActionResult> Retos()
        {
            var usuarioId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            // Obtener el equipo del usuario logueado
            var equipoUsuario = await _context.Equipos
                .Include(e => e.Pokemons)
                .FirstOrDefaultAsync(e => e.UsuarioId == usuarioId);

            // Verificar si todos los Pokémon del equipo tienen 0 de vida
            bool todosPokemonsConVida0 = equipoUsuario?.Pokemons.All(p => p.Vida == 0) ?? false;

            // Obtener todos los usuarios con rol "entrenador" en la base de datos
            var todosUsuarios = await _context.Users.ToListAsync();

            // Filtrar usuarios con rol "entrenador" y que no sean el usuario logueado
            var entrenadores = new List<EntrenadorViewModel>();

            foreach (var usuario in todosUsuarios)
            {
                if (await _userManager.IsInRoleAsync(usuario, "entrenador") && usuario.Id != usuarioId)
                {
                    var equipo = await _context.Equipos
                        .Include(e => e.Pokemons)
                        .FirstOrDefaultAsync(e => e.UsuarioId == usuario.Id);

                    entrenadores.Add(new EntrenadorViewModel
                    {
                        UsuarioId = usuario.Id,
                        UserName = usuario.UserName,
                        HasTeam = equipo != null,
                        EquipoNombre = equipo?.Nombre,
                        TodosPokemonsConVida0 = equipo?.Pokemons.All(p => p.Vida == 0) ?? false
                    });
                }
            }

            var model = new RetosViewModel
            {
                CurrentUserTeam = equipoUsuario,
                Entrenadores = entrenadores,
                TodosPokemonsConVida0 = todosPokemonsConVida0
            };

            return View(model);
        }



        [HttpPost]
        [Authorize(Roles = "entrenador")]
        public async Task<IActionResult> EliminarEquipo()
        {
            var usuarioId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            var equipo = await _context.Equipos
                .Include(e => e.Pokemons)
                .FirstOrDefaultAsync(e => e.UsuarioId == usuarioId);

            if (equipo != null)
            {
                _context.Pokemons.RemoveRange(equipo.Pokemons);
                _context.Equipos.Remove(equipo);
                await _context.SaveChangesAsync();
                TempData["Resultado"] = "Tu equipo ha sido eliminado exitosamente.";
            }
            else
            {
                TempData["Resultado"] = "No tienes un equipo que eliminar.";
            }

            return RedirectToAction("Retos");
        }




        [Route("Chat")]
        [Authorize(Roles = "entrenador")]
        public IActionResult Chat()
        {
            var messages = _context.ChatMessages
               .OrderBy(m => m.Timestamp)
               .ToList();
            return View(messages);
        }



        [Route("Curar")]
        [Authorize(Roles = "enfermeria")]
        public async Task<IActionResult> Curar()
        {
            // Obtener los equipos con al menos un Pokémon cuya vida sea 0
            var equipos = await _context.Equipos
                .Where(e => e.Pokemons.Any(p => p.Vida == 0))
                .Select(e => new CurarViewModel
                {
                    // Se obtiene el nombre de usuario usando el UserManager
                    EntrenadorNombre = _context.Users.FirstOrDefault(u => u.Id == e.UsuarioId).UserName,
                    EquipoId = e.Id,
                    EquipoNombre = e.Nombre,
                    Pokemons = e.Pokemons.Where(p => p.Vida == 0).Select(p => new PokemonViewModel
                    {
                        PokemonId = p.Id,
                        Nombre = p.Nombre,
                        ImagenUrl = p.ImagenUrl,
                        Vida = p.Vida
                    }).ToList()
                }).ToListAsync();

            return View(equipos);
        }



        [HttpPost]
        [Authorize(Roles = "enfermeria")]
        public async Task<IActionResult> CurarEquipo(int equipoId)
        {
            var equipo = await _context.Equipos
                .Include(e => e.Pokemons)
                .FirstOrDefaultAsync(e => e.Id == equipoId);

            if (equipo == null)
            {
                return NotFound();
            }

            using (var httpClient = new HttpClient())
            {
                foreach (var pokemon in equipo.Pokemons.Where(p => p.Vida == 0))
                {
                    var response = await httpClient.GetStringAsync($"https://pokeapi.co/api/v2/pokemon/{pokemon.Nombre.ToLower()}");
                    var data = JObject.Parse(response);

                    // Obtener el valor de "hp" del Pokémon desde la API
                    var hp = data["stats"].First(stat => (string)stat["stat"]["name"] == "hp")["base_stat"].Value<int>();

                    // Restablecer la vida del Pokémon
                    pokemon.Vida = hp;
                }
            }

            await _context.SaveChangesAsync();

            TempData["Resultado"] = "El equipo ha sido curado con éxito.";
            return RedirectToAction(nameof(Curar));
        }





        // Acción para mostrar usuarios entrenadores
        [Route("AdminUsuarios")]
        [Authorize(Roles = "administrador")]
        public async Task<IActionResult> AdminUsuarios()
        {
            var todosLosUsuarios = await _context.Users.ToListAsync();

            var entrenadores = todosLosUsuarios
                .Where(u => _userManager.IsInRoleAsync(u, "entrenador").Result)
                .Select(u => new
                {
                    u.Id,
                    u.UserName,
                    u.Email
                })
                .ToList();

            return View(entrenadores);
        }


        // Acción para editar un usuario
        [HttpPost]
        [Authorize(Roles = "administrador")]
        public async Task<IActionResult> EditarUsuario(string id, string nuevoNombre, string nuevoEmail)
        {
            var usuario = await _userManager.FindByIdAsync(id);
            if (usuario != null)
            {
                usuario.UserName = nuevoNombre;
                usuario.Email = nuevoEmail;

                var result = await _userManager.UpdateAsync(usuario);
                if (result.Succeeded)
                {
                    TempData["Resultado"] = "Usuario actualizado con éxito.";
                }
                else
                {
                    TempData["Resultado"] = "Error al actualizar el usuario.";
                }
            }
            return RedirectToAction(nameof(AdminUsuarios));
        }

        // Acción para eliminar un usuario
        [HttpPost]
        [Authorize(Roles = "administrador")]
        public async Task<IActionResult> EliminarUsuario(string id)
        {
            var usuario = await _userManager.FindByIdAsync(id);
            if (usuario != null)
            {
                var result = await _userManager.DeleteAsync(usuario);
                if (result.Succeeded)
                {
                    TempData["Resultado"] = "Usuario eliminado con éxito.";
                }
                else
                {
                    TempData["Resultado"] = "Error al eliminar el usuario.";
                }
            }
            return RedirectToAction(nameof(AdminUsuarios));
        }



        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
