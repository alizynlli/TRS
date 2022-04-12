using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Linq;
using System.Threading.Tasks;
using TRS.Core.Constants.Enums;
using TRS.Data.Models;
using TRS.Web.Models;
using TRS.Web.Services;
using TRS.Web.ViewModels.Administration;

namespace TRS.Web.Controllers
{
    public class AdministrationController : Controller
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly AdministrationService _service;
        private readonly ILogger<AdministrationController> _logger;

        public AdministrationController(UserManager<ApplicationUser> userManager, RoleManager<IdentityRole> roleManager, AdministrationService service, ILogger<AdministrationController> logger)
        {
            _userManager = userManager;
            _roleManager = roleManager;
            _service = service;
            _logger = logger;
        }

        [HttpGet]
        [AllowAnonymous]
        public IActionResult CreateSuperAdmin()
        {
            //return RedirectToAction("AccessDenied");
            return View();
        }

        [HttpPost]
        [AllowAnonymous]
        public async Task<IActionResult> CreateSuperAdmin(CreateSuperAdminViewModel model)
        {
            //return RedirectToAction("AccessDenied");
            if (ModelState.IsValid)
            {
                var user = new ApplicationUser
                {
                    FirstName = model.FirstName,
                    LastName = model.LastName,
                    UserName = model.UserName,
                    Email = model.Email
                };

                var createResult = await _userManager.CreateAsync(user, model.Password);

                if (createResult.Succeeded)
                {
                    var addToRoleResult = await _userManager.AddToRoleAsync(user, "Super Admin");

                    if (addToRoleResult.Succeeded)
                    {
                        _logger.LogInformation("Super admin created successfully.");

                        return RedirectToAction("Index", "Home");
                    }
                    else
                    {
                        _logger.LogError($"Exception thrown while super admin adding to role. Exception message: {addToRoleResult.Errors?.FirstOrDefault()}");
                     
                        return View("Error", new ErrorModel { ErrorMessage = $"Super admin rola əlavə edilərkən xəta yarandı. Xəta mesajı: {addToRoleResult.Errors?.FirstOrDefault()}"});
                    }
                }

                foreach (var error in createResult.Errors)
                {
                    ModelState.AddModelError("", error.Description);
                }
            }

            return View(model);
        }

        [HttpGet]
        [AllowAnonymous]
        public IActionResult CreateRole()
        {
            //return RedirectToAction("AccessDenied");
            return View();
        }

        [HttpPost]
        [AllowAnonymous]
        public async Task<IActionResult> CreateRole(CreateRoleViewModel model)
        {
            //return RedirectToAction("AccessDenied");
            if (ModelState.IsValid)
            {
                var role = new IdentityRole
                {
                    Name = model.Name
                };

                var result = await _roleManager.CreateAsync(role);
                if (result.Succeeded)
                {
                    return RedirectToAction("Index", "Home");
                }

                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError("", error.Description);
                }
            }

            return View();
        }

        [AllowAnonymous]
        public IActionResult AccessDenied()
        {
            return View();
        }

        [Authorize(Roles = "Super Admin")]
        public IActionResult TaskList()
        {
            return View();
        }

        [Authorize(Roles = "Super Admin")]
        public async Task<IActionResult> LoadTasks(byte taskStatus, DateTime firstDate)
        {
            try
            {
                IActionResult RedirectToErrorPage(Core.Helpers.ActionResult result)
                {
                    var errorModel = new ErrorModel
                    {
                        ErrorMessage = result.ErrorMessages?.FirstOrDefault()
                    };

                    _logger.LogError(errorModel.ErrorMessage + ". Controller: Administration; Action: LoadTasks");

                    return View("Error", errorModel);
                }

                var draw = Request.Form["draw"].FirstOrDefault();
                var start = Request.Form["start"].FirstOrDefault();
                var length = Request.Form["length"].FirstOrDefault();
                var sortColumn = Request.Form["columns[" + Request.Form["order[0][column]"].FirstOrDefault() + "][name]"].FirstOrDefault();
                var sortColumnDirection = Request.Form["order[0][dir]"].FirstOrDefault();
                var searchValue = Request.Form["search[value]"].FirstOrDefault();
                var pageSize = length != null ? Convert.ToInt32(length) : 0;
                var skip = start != null ? Convert.ToInt32(start) : 0;

                object data = null;

                if (taskStatus == (byte)ClientTaskStatuses.NotSeen)
                {
                    var result = await _service.GetNewTasks(draw, sortColumn, sortColumnDirection, searchValue, skip, pageSize, firstDate);
                    if (result.IsFailed) return RedirectToErrorPage(result);
                    data = result.Data;
                }

                else if (taskStatus == (byte)ClientTaskStatuses.UnderConsideration)
                {
                    var result = await _service.GetUnderConsiderationTasks(draw, sortColumn, sortColumnDirection, searchValue, skip, pageSize, firstDate);
                    if (result.IsFailed) return RedirectToErrorPage(result);
                    data = result.Data;
                }

                else if (taskStatus == (byte)ClientTaskStatuses.Completed || taskStatus == (byte)ClientTaskStatuses.Confirmed)
                {
                    var result = await _service.GetCompletedTasks(draw, sortColumn, sortColumnDirection, searchValue, skip, pageSize, firstDate);
                    if (result.IsFailed) return RedirectToErrorPage(result);
                    data = result.Data;
                }

                return Ok(data);
            }
            catch (Exception e)
            {
                var errorModel = new ErrorModel
                {
                    ErrorMessage = e.Message
                };

                return View("Error", errorModel);
            }
        }

        [Authorize(Roles = "Super Admin")]
        [HttpGet]
        public async Task<IActionResult> ShowTaskDetails(string taskId)
        {
            if (string.IsNullOrEmpty(taskId))
            {
                var errorModel = new ErrorModel
                {
                    ErrorMessage = "Tapırıq id-si boş ola bilməz!"
                };

                _logger.LogError($"Empty task id. Current User: {User.Identity.Name}");

                return View("Error", errorModel);
            }

            var result = await _service.GetTaskDetails(taskId);

            if (result.IsFailed)
            {
                var errorModel = new ErrorModel
                {
                    ErrorMessage = result.ErrorMessages?.FirstOrDefault()
                };

                _logger.LogError($"Exception thrown while reading task details. Exception message: {result.ErrorMessages?.FirstOrDefault()}");

                return View("Error", errorModel);
            }

            return View(result.Data);
        }
    }
}
