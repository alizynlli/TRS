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
using TRS.Web.ViewModels.ClientUser;

namespace TRS.Web.Controllers
{
    public class ClientUserController : Controller
    {
        private readonly ClientUserService _service;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ILogger<ClientUserController> _logger;

        public ClientUserController(ClientUserService service, UserManager<ApplicationUser> userManager, ILogger<ClientUserController> logger)
        {
            _service = service;
            _userManager = userManager;
            _logger = logger;
        }

        [Authorize(Roles = "Super Admin")]
        [HttpGet]
        public async Task<IActionResult> Create()
        {
            var modelResult = await _service.GetNewClientUser();

            if (modelResult.IsFailed)
            {
                var errorModel = new ErrorModel
                {
                    ErrorMessage = modelResult.ErrorMessages?.FirstOrDefault()
                };

                _logger.LogError($"Exception thrown while getting new client user model. Exception message: {errorModel.ErrorMessage}");

                return View("Error", errorModel);
            }

            return View(modelResult.Data);
        }

        [Authorize(Roles = "Super Admin")]
        [HttpPost]
        public async Task<IActionResult> Create(CreateClientUserViewModel model)
        {
            if (ModelState.IsValid)
            {
                var result = await _service.CreateAsync(model);
                if (result.IsFailed)
                {
                    var errorModel = new ErrorModel
                    {
                        ErrorMessage = result.ErrorMessages?.FirstOrDefault()
                    };

                    _logger.LogError($"Exception thrown while creating new client user model. Exception message: {errorModel.ErrorMessage}");

                    return View("Error", errorModel);
                }

                return RedirectToAction("List");
            }

            return View(model);
        }

        [Authorize(Roles = "Super Admin")]
        [HttpGet]
        public IActionResult List()
        {
            return View();
        }

        [Authorize(Roles = "Super Admin")]
        public async Task<IActionResult> LoadClientUsers()
        {
            try
            {
                var draw = Request.Form["draw"].FirstOrDefault();
                var start = Request.Form["start"].FirstOrDefault();
                var length = Request.Form["length"].FirstOrDefault();
                var sortColumn = Request.Form["columns[" + Request.Form["order[0][column]"].FirstOrDefault() + "][name]"].FirstOrDefault();
                var sortColumnDirection = Request.Form["order[0][dir]"].FirstOrDefault();
                var searchValue = Request.Form["search[value]"].FirstOrDefault();
                var pageSize = length != null ? Convert.ToInt32(length) : 0;
                var skip = start != null ? Convert.ToInt32(start) : 0;

                var modelResult = await _service.GetClientUsers(draw, sortColumn, sortColumnDirection, searchValue, skip, pageSize);

                if (modelResult.IsFailed)
                {
                    var errorModel = new ErrorModel
                    {
                        ErrorMessage = modelResult.ErrorMessages?.FirstOrDefault()
                    };

                    _logger.LogError($"Exception thrown while getting client users. Exception message: {errorModel.ErrorMessage}. Controller: ClientUser, Action: LoadClientUsers");

                    return View("Error", errorModel);
                }

                return Ok(modelResult.Data);
            }
            catch (Exception e)
            {
                var errorModel = new ErrorModel
                {
                    ErrorMessage = e.Message
                };

                _logger.LogError($"Exception thrown while getting client users. Exception message: {errorModel.ErrorMessage}. Controller: ClientUser, Action: LoadClientUsers");

                return View("Error", errorModel);
            }
        }

        [Authorize(Roles = "Client")]
        [HttpGet]
        public async Task<IActionResult> CreateTask()
        {
            var result = await _service.GetNewTaskModelAsync();

            if (result.IsFailed)
            {
                var errorModel = new ErrorModel
                {
                    ErrorMessage = result.ErrorMessages?.FirstOrDefault()
                };

                _logger.LogError($"Exception thrown while getting new task model. Exception message: {errorModel.ErrorMessage}. Controller: ClientUser, Action: CreateTask");

                return View("Error", errorModel);
            }

            var model = result.Data;

            return View(model);
        }

        [Authorize(Roles = "Client")]
        [HttpPost]
        public async Task<IActionResult> CreateTask(CreateClientTaskViewModel model)
        {
            if (ModelState.IsValid)
            {
                var user = await _userManager.GetUserAsync(User);

                var result = await _service.CreateClientTaskAsync(model, user.Id);

                if (result.IsFailed)
                {
                    var errorModel = new ErrorModel
                    {
                        ErrorMessage = result.ErrorMessages?.FirstOrDefault()
                    };

                    _logger.LogError($"Exception thrown while creating new task by client user. Exception message: {errorModel.ErrorMessage}. Controller: ClientUser, Action: CreateTask");

                    return View("Error", errorModel);
                }

                return RedirectToAction("ClientTaskList");
            }

            return View(model);
        }

        [Authorize(Roles = "Client")]
        [HttpGet]
        public async Task<IActionResult> EditTask(string taskId)
        {
            var user = await _userManager.GetUserAsync(User);

            var result = await _service.GetExistingTaskModelAsync(taskId, user.Id);

            if (result.IsFailed)
            {
                var errorModel = new ErrorModel
                {
                    ErrorMessage = result.ErrorMessages?.FirstOrDefault()
                };

                _logger.LogError($"Exception thrown while getting existing task model. Exception message: {errorModel.ErrorMessage}. Controller: ClientUser, Action: EditTask");

                return View("Error", errorModel);
            }

            var model = result.Data;

            return View(model);
        }

        [Authorize(Roles = "Client")]
        [HttpPost]
        public async Task<IActionResult> EditTask(EditClientTaskViewModel model)
        {
            if (ModelState.IsValid)
            {
                var user = await _userManager.GetUserAsync(User);

                var result = await _service.EditClientTaskAsync(model, user.Id);

                if (result.IsFailed)
                {
                    var errorModel = new ErrorModel
                    {
                        ErrorMessage = result.ErrorMessages?.FirstOrDefault()
                    };

                    _logger.LogError($"Exception thrown while editing task by client user. Exception message: {errorModel.ErrorMessage}. Controller: ClientUser, Action: EditTask");

                    return View("Error", errorModel);
                }

                return RedirectToAction("ClientTaskList");
            }

            return View(model);
        }


        [Authorize(Roles = "Client")]
        [HttpPost]
        public async Task<IActionResult> ConfirmTask(string taskId)
        {
            var user = await _userManager.GetUserAsync(User);

            var result = await _service.ConfirmTask(taskId, user.Id);

            if (result.IsFailed)
            {
                var errorModel = new ErrorModel
                {
                    ErrorMessage = result.ErrorMessages?.FirstOrDefault()
                };

                _logger.LogError($"Exception thrown while confirming task. Exception message: {errorModel.ErrorMessage}. Controller: ClientUser, Action: ConfirmTask");

                return View("Error", errorModel);
            }

            return RedirectToAction("ClientTaskList");
        }

        [Authorize(Roles = "Client")]
        [HttpGet]
        public IActionResult ClientTaskList()
        {
            return View();
        }

        [Authorize(Roles = "Client")]
        public async Task<IActionResult> LoadClientTasks(DateTime firstDate)
        {
            try
            {
                var user = await _userManager.FindByNameAsync(User.Identity.Name);

                var draw = Request.Form["draw"].FirstOrDefault();
                var start = Request.Form["start"].FirstOrDefault();
                var length = Request.Form["length"].FirstOrDefault();
                var sortColumn = Request.Form["columns[" + Request.Form["order[0][column]"].FirstOrDefault() + "][name]"].FirstOrDefault();
                var sortColumnDirection = Request.Form["order[0][dir]"].FirstOrDefault();
                var searchValue = Request.Form["search[value]"].FirstOrDefault();
                var pageSize = length != null ? Convert.ToInt32(length) : 0;
                var skip = start != null ? Convert.ToInt32(start) : 0;

                var modelResult = await _service.GetClientTasks(draw, sortColumn, sortColumnDirection, searchValue, skip, pageSize, user.Id, firstDate);

                if (modelResult.IsFailed)
                {
                    var errorModel = new ErrorModel
                    {
                        ErrorMessage = modelResult.ErrorMessages?.FirstOrDefault()
                    };

                    _logger.LogError($"Exception thrown while getting tasks. Exception message: {errorModel.ErrorMessage}. Controller: ClientUser, Action: LoadClientTasks");

                    return View("Error", errorModel);
                }

                return Ok(modelResult.Data);
            }
            catch (Exception e)
            {
                var errorModel = new ErrorModel
                {
                    ErrorMessage = e.Message
                };

                _logger.LogError($"Exception thrown while getting tasks. Exception message: {errorModel.ErrorMessage}. Controller: ClientUser, Action: LoadClientTasks");

                return View("Error", errorModel);
            }
        }

        [Authorize(Roles = "Client")]
        [HttpGet]
        public async Task<IActionResult> ShowTaskDetails(string taskId)
        {
            if (string.IsNullOrEmpty(taskId))
            {
                var errorModel = new ErrorModel
                {
                    ErrorMessage = "Sorğunun parametrləri doğru daxil edilməyib!"
                };

                _logger.LogError($"Incorrect action arguments. TaskId is null or empty. Controller: ClientUser, Action: ShowTaskDetails, Current User: {User.Identity.Name}");

                return View("Error", errorModel);
            }

            var result = await _service.GetTaskDetails(taskId);

            if (result.IsFailed)
            {
                var errorModel = new ErrorModel
                {
                    ErrorMessage = result.ErrorMessages?.FirstOrDefault()
                };

                _logger.LogError($"Exception thrown while getting task details. Exception message: {errorModel.ErrorMessage}. Controller: ClientUser, Action: ShowTaskDetails");

                return View("Error", errorModel);
            }

            return View(result.Data);
        }
    }
}
