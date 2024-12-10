using Authentication_Application.Services;
using Authentication_Application.ViewModels;
using Azure.Core;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace Authentication_Application.Controllers
{
    public class AccountController : Controller
    {
        private readonly IUserService _userServices;
        private readonly ILogger<HomeController> _logger;
        public AccountController(IUserService userServices, ILogger<HomeController> logger)
        {
            _userServices = userServices;
            _logger = logger;
        }
        [HttpGet]
        public async Task <IActionResult> Register()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Register(RegisterViewModel request)
        {
            if (ModelState.IsValid)
            {
                bool registrationStatus = await _userServices.RegisterUser(request);
                if (registrationStatus)
                {
                    ModelState.Clear();
                    TempData["Success"] = "Registration Successful!";
                    return View();
                }
                else
                {
                    TempData["Fail"] = "Registration Failed";
                    return View(request);
                }
            }
            return View(request);
        }

        [HttpGet]
        public async Task<IActionResult> Login()
        {
            return View();
        }
        [HttpPost]
        public async Task<IActionResult> Login(LoginViewModel request)
        {
            if (ModelState.IsValid)
            {
                bool isAuthenticated = await _userServices.AuthenticateUser(request);
                if (isAuthenticated)
                {
                    var user = await _userServices.GetUser(i => i.Username == request.Username);

                    var claims = new List<Claim>
                    {
                        new Claim(ClaimTypes.Name, request.Username)
                    };
                    ClaimsIdentity userIdentity = new ClaimsIdentity(claims, "Login");
                    ClaimsPrincipal principle = new ClaimsPrincipal(userIdentity);

                    await HttpContext.SignInAsync(principle);
                    if (user.IsTwoFactorEnabled)
                    {
                        if (user.SecretKey == null)
                        {
                            return RedirectToAction("EnableAuthenticator", "Account", new { username = user.Username });
                        }
                        else
                        {
                            return View("LoginTwoStep", new TwoStepLoginViewModel { Username = user.Username });
                        }
                    }
                    return RedirectToAction("Home", "Home", new {username = request.Username});
                }
                else
                {
                    TempData["UserLoginFailed"] = "Please enter correct Id and Password";
                    return View(request);
                }
            }
            return View(request);
        }

        [HttpPost]
        public async Task<IActionResult> LoginTwoStep(TwoStepLoginViewModel request)
        {
            bool isVerified = await _userServices.VerifyTwoFactorAuthentication(request);
            if (isVerified)
            {
                var claims = new List<Claim>
                {
                    new Claim(ClaimTypes.Name, request.Username)
                };
                ClaimsIdentity userIdentity = new ClaimsIdentity(claims, "login");
                ClaimsPrincipal principal = new ClaimsPrincipal(userIdentity);

                await HttpContext.SignInAsync(principal);
                return RedirectToAction("Home", "Home", new {username = request.Username});
            }

            ModelState.AddModelError("", "Invalid verification code");
            return View(request);
        }

        [HttpGet, Authorize]
        public async Task<IActionResult> EnableAuthenticator(string username)
        {
            EnableAuthenticatorViewModel model = new EnableAuthenticatorViewModel();
            var user = await _userServices.GetUser(i => i.Username == username);
            model.Username = username;
            (string secretKey, string qrCodeUrl) = await _userServices.GenerateTwoFactorInfo(model.Username);
            model.SecretKey = secretKey;
            model.AuthenticatorUri = qrCodeUrl;

            return View(model);
            
        }

        [HttpPost, Authorize]
        public async Task<IActionResult> EnableAuthenticator(EnableAuthenticatorViewModel model)
        {
            var res = await _userServices.EnableAutheticator(model);

            if (res)
            {
                return RedirectToAction("Home", "Home", new {username = model.Username});
            }
            ModelState.AddModelError("", "Invalid Code");
            return View(model);
        }

        [HttpGet, Authorize]
        public async Task<IActionResult> DisableAuthenticator()
        {
            string username = User.Identity.Name;
            await _userServices.DisableAutheticator(username);
            return RedirectToAction("Index", "Home");
        }


        [HttpPost]
        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync();
            return RedirectToAction("Index", "Home");
        }
    }
}
