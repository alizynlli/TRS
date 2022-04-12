using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Linq;
using System.Threading.Tasks;
using TRS.Data.Models;
using TRS.Data.Repositories.Abstract;
using TRS.Web.Models;
using TRS.Web.ViewModels.ClientTaskType;

namespace TRS.Web.Controllers
{
    public class ClientTaskController : Controller
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogger<ClientTaskController> _logger;

        public ClientTaskController(IUnitOfWork unitOfWork, ILogger<ClientTaskController> logger)
        {
            _unitOfWork = unitOfWork;
            _logger = logger;
        }

        [Authorize(Roles = "Super Admin")]
        [HttpGet]
        public IActionResult ClientTaskTypeList()
        {
            return View();
        }

        [Authorize(Roles = "Super Admin")]
        public async Task<IActionResult> LoadClientTaskTypes()
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

                var taskTypeResult = await _unitOfWork.ClientTaskTypeRepo.GetAllAsync();

                if (taskTypeResult.IsFailed)
                {
                    var errorModel = new ErrorModel
                    {
                        ErrorMessage = taskTypeResult.ErrorMessages?.FirstOrDefault()
                    };

                    _logger.LogError(errorModel.ErrorMessage + ". Controller: ClientTask; Action: LoadClientTaskTypes");

                    return View("Error", errorModel);
                }

                var taskTypeData = from tempClient in taskTypeResult.Data select tempClient;
                if (!(string.IsNullOrEmpty(sortColumn) && string.IsNullOrEmpty(sortColumnDirection)))
                {
                    //ClientData = ClientData.OrderBy(sortColumn + " " + sortColumnDirection);
                }
                if (!string.IsNullOrEmpty(searchValue))
                {
                    taskTypeData = taskTypeData.Where(m => m.Name.Contains(searchValue));
                }

                int recordsTotal = taskTypeData.Count();
                var data = taskTypeData.Skip(skip).Take(pageSize).ToList();
                var jsonData = new { draw, recordsFiltered = recordsTotal, recordsTotal, data };
                return Ok(jsonData);
            }
            catch (Exception exception)
            {
                var errorModel = new ErrorModel
                {
                    ErrorMessage = exception.Message
                };

                _logger.LogError(errorModel.ErrorMessage + ". Controller: ClientTask; Action: LoadClientTaskTypes");

                return View("Error", errorModel);
            }
        }

        [Authorize(Roles = "Super Admin")]
        [HttpGet]
        public IActionResult CreateTaskType()
        {
            return View();
        }

        [Authorize(Roles = "Super Admin")]
        [HttpPost]
        public async Task<IActionResult> CreateTaskType(CreateClientTaskTypeViewModel model)
        {
            if (ModelState.IsValid)
            {
                var taskType = new ClientTaskType
                {
                    Name = model.TypeName
                };

                var result = await _unitOfWork.ClientTaskTypeRepo.AddAsync(taskType);
                if (result.IsFailed)
                {
                    foreach (var error in result.ErrorMessages)
                    {
                        ModelState.AddModelError(string.Empty, error);
                    }

                    _logger.LogError(result.ErrorMessages?.FirstOrDefault() + ". Controller: ClientTask; Action: ClientTaskType");

                    return View();
                }

                return RedirectToAction("ClientTaskTypeList");
            }

            return View(model);
        }

        [Authorize(Roles = "Super Admin")]
        [HttpGet]
        public async Task<IActionResult> EditTaskType(string taskTypeId)
        {
            if (string.IsNullOrEmpty(taskTypeId))
            {
                _logger.LogWarning("taskTypeId parameter is null or empty on EditTaskType action of ClientTask controller.");

                return View("Error", new ErrorModel { ErrorMessage = "Sorğunun parametrləri düzgün daxil edilməyib." });
            }

            var taskTypeResult = await _unitOfWork.ClientTaskTypeRepo.FindByIdAsync(taskTypeId);

            if (taskTypeResult.IsSucceed && taskTypeResult.Data != null)
            {
                var model = new EditClientTaskTypeViewModel
                {
                    Id = taskTypeId,
                    TypeName = taskTypeResult.Data.Name
                };

                return View(model);
            }

            _logger.LogError(taskTypeResult.ErrorMessages?.FirstOrDefault() + ". Controller: ClientTask, Action: EditTaskType");

            return View("Error", new ErrorModel { ErrorMessage = $"Tapşırıq tipi güncəllənərkən xəta yarandı. Xəta mesajı: {taskTypeResult.ErrorMessages?.FirstOrDefault()}" });
        }

        [Authorize(Roles = "Super Admin")]
        [HttpPost]
        public async Task<IActionResult> EditTaskType(EditClientTaskTypeViewModel model)
        {
            if (ModelState.IsValid)
            {
                var taskTypeResult = await _unitOfWork.ClientTaskTypeRepo.FindByIdAsync(model.Id);

                if (taskTypeResult.IsFailed)
                {
                    foreach (var error in taskTypeResult.ErrorMessages)
                    {
                        ModelState.AddModelError(string.Empty, error);
                    }

                    _logger.LogError($"Exception thrown while reading task type. Exception message: {taskTypeResult.ErrorMessages?.FirstOrDefault()}. Controller: ClientTask, Action: EditTaskType");

                    return View("Error", new ErrorModel { ErrorMessage = $"Tapşırıq tipi gətirilərkən xəta yarandı. Xəta mesajı: {taskTypeResult.ErrorMessages?.FirstOrDefault()}" });
                }

                var taskType = taskTypeResult.Data;

                taskType.Name = model.TypeName;

                var updateResult = await _unitOfWork.ClientTaskTypeRepo.UpdateAsync(taskType);
                if (updateResult.IsFailed)
                {
                    foreach (var error in updateResult.ErrorMessages)
                    {
                        ModelState.AddModelError(string.Empty, error);
                    }

                    _logger.LogError($"Exception thrown while editing task type. Exception message: {taskTypeResult.ErrorMessages?.FirstOrDefault()}. Controller: ClientTask, Action: EditTaskType");

                    return View("Error", new ErrorModel { ErrorMessage = $"Tapşırıq tipi güncəllənərkən xəta yarandı. Xəta mesajı: {taskTypeResult.ErrorMessages?.FirstOrDefault()}" });
                }

                return RedirectToAction("ClientTaskTypeList");
            }

            return View(model);
        }
    }
}
