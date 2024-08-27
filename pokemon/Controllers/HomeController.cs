using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
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
        public IActionResult Curar()
        {
            return View();
        }

        [Route("AdminUsuarios")]
        [Authorize(Roles = "administrador")]
        public IActionResult AdminUsuarios()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
