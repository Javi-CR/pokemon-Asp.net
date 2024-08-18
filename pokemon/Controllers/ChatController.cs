using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using pokemon.Data;
using pokemon.Models;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace pokemon.Controllers
{
    [Authorize]
    public class ChatController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<IdentityUser> _userManager;

        public ChatController(ApplicationDbContext context, UserManager<IdentityUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        // Acción para la vista del chat
        public IActionResult Index()
        {
            return View();
        }


        // Acción para manejar el envío de mensajes
        [HttpPost]
        public async Task<IActionResult> SendMessage(string message)
        {
            if (!string.IsNullOrWhiteSpace(message))
            {
                var chatMessage = new ChatMessage
                {
                    UserName = _userManager.GetUserName(User),
                    Message = message,
                    Timestamp = DateTime.Now
                };

                _context.ChatMessages.Add(chatMessage);
                await _context.SaveChangesAsync();
            }

            return RedirectToAction("Index", "Chat"); // Redirige a la vista de selección
        }
    }
}
