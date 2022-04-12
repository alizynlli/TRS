using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Linq;
using System.Linq.Dynamic.Core;
using System.Threading.Tasks;
using TRS.Core.Constants.Enums;
using TRS.Data.Models;
using TRS.Web.Models;
using TRS.Web.Services;
using TRS.Web.ViewModels.Personnel;

namespace TRS.Web.Controllers
{
    public class PersonnelController : Controller
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly PersonnelService _service;
        private readonly ILogger<PersonnelController> _logger;

        public PersonnelController(UserManager<ApplicationUser> userManager, PersonnelService service, ILogger<PersonnelController> logger)
        {
            _userManager = userManager;
            _service = service;
            _logger = logger;
        }

        [Authorize(Roles = "Super Admin")]
        public IActionResult List()
        {
            return View();
        }

        [Authorize(Roles = "Super Admin")]
        public async Task<IActionResult> LoadPersonnel()
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

                var modelResult = await _service.GetPersonnels(draw, sortColumn, sortColumnDirection, searchValue, skip, pageSize);

                if (modelResult.IsFailed)
                {
                    var errorModel = new ErrorModel
                    {
                        ErrorMessage = modelResult.ErrorMessages?.FirstOrDefault()
                    };

                    _logger.LogError($"Exception thrown while reading personnels. Exception message: {errorModel.ErrorMessage}. Controller: Personnel, action: LoadPersonnel");

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

                _logger.LogError($"Exception thrown while reading personnels. Exception message: {errorModel.ErrorMessage}. Controller: Personnel, action: LoadPersonnel");

                return View("Error", errorModel);
            }
        }

        [Authorize(Roles = "Super Admin")]
        [HttpGet]
        public IActionResult Create()
        {
            return View();
        }

        [Authorize(Roles = "Super Admin")]
        [HttpPost]
        public async Task<IActionResult> Create(CreatePersonnelViewModel model)
        {
            if (ModelState.IsValid)
            {
                var result = await _service.Create(model);

                if (result.IsFailed)
                {
                    var errorModel = new ErrorModel
                    {
                        ErrorMessage = result.ErrorMessages?.FirstOrDefault()
                    };

                    _logger.LogError($"Exception thrown while creating personnel. Exception message: {errorModel.ErrorMessage}. Controller: Personnel, action: Create");

                    return View("Error", errorModel);
                }

                return RedirectToAction("List");
            }

            return View(model);
        }

        [Authorize(Roles = "Personnel")]
        public IActionResult TaskList()
        {
            return View();
        }

        [Authorize(Roles = "Personnel")]
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

                    _logger.LogError($"Exception thrown while reading tasks by personnel. Exception message: {errorModel.ErrorMessage}. Controller: Personnel, action: LoadTasks");

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

                var personnel = await _userManager.GetUserAsync(User);
                object data = null;

                if (taskStatus == (byte)ClientTaskStatuses.NotSeen)
                {
                    var result = await _service.GetNewTasks(draw, sortColumn, sortColumnDirection, searchValue, skip, pageSize, firstDate);
                    if (result.IsFailed) return RedirectToErrorPage(result);
                    data = result.Data;
                }

                else if (taskStatus == (byte)ClientTaskStatuses.UnderConsideration)
                {
                    var result = await _service.GetUnderConsiderationTasks(draw, sortColumn, sortColumnDirection, searchValue, skip, pageSize, personnel.Id, firstDate);
                    if (result.IsFailed) return RedirectToErrorPage(result);
                    data = result.Data;
                }

                else if (taskStatus == (byte)ClientTaskStatuses.Completed || taskStatus == (byte)ClientTaskStatuses.Confirmed)
                {
                    var result = await _service.GetCompletedTasks(draw, sortColumn, sortColumnDirection, searchValue, skip, pageSize, personnel.Id, firstDate);
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

                _logger.LogError($"Exception thrown while reading tasks by personnel. Exception message: {errorModel.ErrorMessage}. Controller: Personnel, action: LoadTasks");

                return View("Error", errorModel);
            }
        }

        [Authorize(Roles = "Personnel")]
        [HttpPost]
        public async Task<IActionResult> TakeOnTheTask(string taskId)
        {
            var user = await _userManager.GetUserAsync(User);

            var result = await _service.TakeOnTheTask(taskId, user.Id);

            if (result.IsFailed)
            {
                var errorModel = new ErrorModel
                {
                    ErrorMessage = result.ErrorMessages?.FirstOrDefault()
                };

                _logger.LogError($"Exception thrown while taking task by personnel. Exception message: {errorModel.ErrorMessage}. Controller: Personnel, action: TakeOnTheTask");

                return View("Error", errorModel);
            }

            return RedirectToAction("TaskList");
        }

        [Authorize(Roles = "Personnel")]
        [HttpPost]
        public async Task<IActionResult> CompleteTask(string taskId)
        {
            var personnel = await _userManager.GetUserAsync(User);

            var result = await _service.CompleteTask(taskId, personnel.Id);

            if (result.IsFailed)
            {
                var errorModel = new ErrorModel
                {
                    ErrorMessage = result.ErrorMessages?.FirstOrDefault()
                };

                _logger.LogError($"Exception thrown while completing task by personnel. Exception message: {errorModel.ErrorMessage}. Controller: Personnel, action: CompleteTask");

                return View("Error", errorModel);
            }

            return RedirectToAction("TaskList");
        }

        [Authorize(Roles = "Personnel")]
        [HttpGet]
        public async Task<IActionResult> ShowTaskDetails(string taskId)
        {
            if (string.IsNullOrEmpty(taskId))
            {
                var errorModel = new ErrorModel
                {
                    ErrorMessage = "Sorğu parametrləri doğru daxil edilməyib!"
                };

                return View("Error", errorModel);

            }

            var result = await _service.GetTaskDetails(taskId);

            if (result.IsFailed)
            {
                var errorModel = new ErrorModel
                {
                    ErrorMessage = result.ErrorMessages?.FirstOrDefault()
                };

                _logger.LogError($"Exception thrown while getting task details by personnel. Exception message: {errorModel.ErrorMessage}. Controller: Personnel, action: ShowTaskDetails");

                return View("Error", errorModel);
            }

            return View(result.Data);
        }

        [Authorize(Roles = "Personnel")]
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

                _logger.LogError($"Exception thrown while getting new task model by personnel. Exception message: {errorModel.ErrorMessage}. Controller: Personnel, action: CreateTask");

                return View("Error", errorModel);
            }

            var model = result.Data;

            return View(model);
        }

        [Authorize(Roles = "Personnel")]
        [HttpPost]
        public async Task<IActionResult> CreateTask(CreateTaskViewModel model)
        {
            if (ModelState.IsValid)
            {
                var user = await _userManager.GetUserAsync(User);

                var result = await _service.CreateTaskAsync(model, user.Id);

                if (result.IsFailed)
                {
                    var errorModel = new ErrorModel
                    {
                        ErrorMessage = result.ErrorMessages?.FirstOrDefault()
                    };

                    _logger.LogError($"Exception thrown while getting new task model by personnel. Exception message: {errorModel.ErrorMessage}. Controller: Personnel, action: CreateTask");

                    return View("Error", errorModel);
                }

                return RedirectToAction("TaskList");
            }

            return View(model);
        }

        [Authorize(Roles = "Personnel")]
        [HttpGet]
        public async Task<IActionResult> TaskTransfer(string taskId)
        {
            var modelResult = await _service.GetTaskTransferModelAsync(taskId);

            if (modelResult.IsFailed)
            {
                var errorModel = new ErrorModel
                {
                    ErrorMessage = modelResult.ErrorMessages?.FirstOrDefault()
                };

                _logger.LogError($"Exception thrown while getting new TaskTransfer model. Exception message: {errorModel.ErrorMessage}. Controller: Personnel, action: TaskTransfer");

                return View("Error", errorModel);
            }

            var user = await _userManager.GetUserAsync(User);

            var model = modelResult.Data;

            model.PersonnelList.Remove(model.PersonnelList.FirstOrDefault(i => i.Value == user.Id));

            return View(model);
        }

        [Authorize(Roles = "Personnel")]
        [HttpPost]
        public async Task<IActionResult> TaskTransfer(TaskTransferViewModel model)
        {
            if (ModelState.IsValid)
            {
                var user = await _userManager.GetUserAsync(User);

                if(user.Id==model.PersonnelId)
                {
                    ModelState.AddModelError("", "Tapşırığı özünüzə təhvil verə bilməzsiniz. Zəhmət olmasa başqa personal seçin.");
                    return View(model);
                }

                var result = await _service.TaskTransferAsync(model, user.Id);

                if (result.IsFailed)
                {
                    var errorModel = new ErrorModel
                    {
                        ErrorMessage = result.ErrorMessages?.FirstOrDefault()
                    };

                    _logger.LogError($"Exception thrown while getting transferring task by personnel. Exception message: {errorModel.ErrorMessage}. Controller: Personnel, action: TaskTransfer");

                    return View("Error", errorModel);
                }

                return RedirectToAction("TaskList");
            }

            return View(model);
        }
    }
}
