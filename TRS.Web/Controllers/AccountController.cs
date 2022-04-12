using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Linq;
using System.Threading.Tasks;
using TRS.Data.Models;
using TRS.Web.Models;
using TRS.Web.Services;
using TRS.Web.ViewModels.Account;

namespace TRS.Web.Controllers
{
    public class AccountController : Controller
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly AccountService _service;
        private readonly ILogger<AccountController> _logger;

        public AccountController(UserManager<ApplicationUser> userManager,
            SignInManager<ApplicationUser> signInManager, AccountService service, ILogger<AccountController> logger)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _service = service;
            _logger = logger;
        }

        [HttpGet]
        [AllowAnonymous]
        public IActionResult Register()
        {
            return RedirectToAction("AccessDenied", "Administration");
            return View();
        }

        [HttpPost]
        [AllowAnonymous]
        public async Task<IActionResult> Register(RegisterViewModel model)
        {
            return RedirectToAction("AccessDenied", "Administration");
            if (ModelState.IsValid)
            {
                var user = new ApplicationUser
                {
                    FirstName = model.FirstName,
                    LastName = model.LastName,
                    UserName = model.UserName,
                    Email = model.Email
                };

                var result = await _userManager.CreateAsync(user, model.Password);

                if (result.Succeeded)
                {
                    await _signInManager.SignInAsync(user, false);

                    return RedirectToAction("Index", "Home");
                }

                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError("", error.Description);
                }
            }

            return View();
        }

        [HttpGet]
        [AllowAnonymous]
        public IActionResult Login()
        {
            return View();
        }

        [HttpPost]
        [AllowAnonymous]
        public async Task<IActionResult> Login(LoginViewModel viewModel, string returnUrl)
        {
            if (ModelState.IsValid)
            {
                var result = await _signInManager.PasswordSignInAsync(viewModel.UserName, viewModel.Password, false, false);

                if (result.Succeeded)
                {
                    if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
                    {
                        return Redirect(returnUrl);
                    }

                    _logger.LogInformation($"Login succeded. Username: {viewModel.UserName}");
                    return RedirectToAction("Index", "Home");
                }

                _logger.LogInformation($"Login failed. Entered username: {viewModel.UserName}, password: {viewModel.Password}");
                ModelState.AddModelError("", "Login Failed.");
            }

            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Logout()
        {
            await _signInManager.SignOutAsync();
            _logger.LogInformation($"Logout succeded. User: {User.Identity.Name}");

            return RedirectToAction("Login");
        }

        [AcceptVerbs("Get", "Post")]
        [AllowAnonymous]
        public async Task<IActionResult> IsUserNameInUse(string username)
        {
            try
            {
                var user = await _userManager.FindByNameAsync(username);
                if (user == null) return Json(true);

                return Json("İstifadəçi adı artıq istifadə olunur");
            }
            catch (Exception e)
            {
                _logger.LogError($"Exception thrown while checking username. Exception message: {e.Message}. Entered username: {username}");
                return View("Error", new ErrorModel { ErrorMessage = e.Message });
            }
        }

        [Authorize(Roles = "Client, Personnel, Super Admin")]
        [HttpGet]
        public async Task<IActionResult> Edit()
        {
            var user = await _userManager.GetUserAsync(User);

            var model = _service.GetExistingUser(user);

            return View(model);
        }

        [Authorize(Roles = "Client, Personnel, Super Admin")]
        [HttpPost]
        public async Task<IActionResult> Edit(EditUserViewModel model)
        {
            if (ModelState.IsValid)
            {
                var user = await _userManager.GetUserAsync(User);

                if (user.Id != model.Id)
                {
                    _logger.LogWarning($"Access denied. An attempt is made to edit user details by another user. Trying user: {User.Identity.Name}, editing user: {model.UserName}");
                    return RedirectToAction("AccessDenied", "Administration");
                }

                if (user.UserName != model.UserName)
                {
                    var secondUser = await _userManager.FindByNameAsync(model.UserName);
                    if (secondUser != null)
                    {
                        ModelState.AddModelError("", "İstifadəçi adı artıq istifadə olunur.");
                        return View(model);
                    }
                }

                if (user.Email != model.Email && !string.IsNullOrEmpty(model.Email))
                {
                    var secondUser = await _userManager.FindByEmailAsync(model.Email);
                    if (secondUser != null)
                    {
                        ModelState.AddModelError("", "Email artıq istifadə olunur.");
                        return View(model);
                    }
                }

                var checkPasswordResult = await _userManager.CheckPasswordAsync(user, model.CurrentPassword);

                if (!checkPasswordResult)
                {
                    ModelState.AddModelError("", "Cari şifrə doğru deyil.");
                    return View(model);
                }

                var updateResult = await _service.EditAsync(model);

                if (updateResult.IsFailed)
                {
                    var errorModel = new ErrorModel
                    {
                        ErrorMessage = updateResult.ErrorMessages?.FirstOrDefault()
                    };

                    _logger.LogError($"Exception thrown while editing username. Exception message: {updateResult.ErrorMessages?.FirstOrDefault()}");

                    return View("Error", errorModel);
                }

                _logger.LogError($"User details updated successfully. Updated user: {User.Identity.Name}");

                return RedirectToAction("Index", "Home");
            }

            return View(model);
        }
    }
}
