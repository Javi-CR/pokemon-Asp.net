using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using pokemon.Data;
using pokemon.Models;
using System.Diagnostics;

namespace pokemon.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly ApplicationDbContext _context;

        public HomeController(ILogger<HomeController> logger, ApplicationDbContext context)
        {
            _logger = logger;
            _context = context;
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
        [Authorize(Roles = "entrenador")] // Restringir a usuarios con el rol "entrenador"
        public IActionResult TuEquipo()
        {
            return View();
        }

        [Route("Pokemon")]
        [Authorize(Roles = "entrenador")] // Restringir a usuarios con el rol "entrenador"
        public IActionResult Pokemon()
        {
            return View();
        }

        [Route("Chat")]
        [Authorize(Roles = "entrenador")] // Restringir a usuarios con el rol "entrenador"
        public IActionResult Chat()
        {
            var messages = _context.ChatMessages
               .OrderBy(m => m.Timestamp)
               .ToList();
            return View(messages);
        }


        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
