using Authentication_Application.Models;
using Authentication_Application.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;

namespace Authentication_Application.Controllers
{
    [Authorize]
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly IUserService _userServices;

        public HomeController(ILogger<HomeController> logger, IUserService userServices)
        {
            _logger = logger;
            _userServices = userServices;
        }

        public IActionResult Index()
        {
            return RedirectToAction("Login", "Account");
        }
        public async Task<IActionResult> Home(string? username)
        {
            User user = await _userServices.GetUser(i => i.Username == username);
            return View(user);
        }
        public IActionResult Privacy()
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
