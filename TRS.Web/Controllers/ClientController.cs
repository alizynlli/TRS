using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Linq;
using System.Linq.Dynamic.Core;
using System.Threading.Tasks;
using TRS.Web.Models;
using TRS.Web.Services;
using TRS.Web.ViewModels;

namespace TRS.Web.Controllers
{
    public class ClientController : Controller
    {
        private readonly ClientService _service;
        private readonly ILogger<ClientController> _logger;

        public ClientController(ClientService service, ILogger<ClientController> logger)
        {
            _service = service;
            _logger = logger;
        }

        [Authorize(Roles = "Super Admin")]
        public IActionResult List()
        {
            return View();
        }

        [Authorize(Roles = "Super Admin")]
        public async Task<IActionResult> LoadClients()
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

                var modelResult = await _service.GetClients(draw, sortColumn, sortColumnDirection, searchValue, skip, pageSize);

                if (modelResult.IsFailed)
                {
                    var errorModel = new ErrorModel
                    {
                        ErrorMessage = modelResult.ErrorMessages?.FirstOrDefault()
                    };

                    _logger.LogError(errorModel.ErrorMessage + ". Controller: Client; Action: LoadClients");

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

                _logger.LogError(e.Message + ". Controller: Client; Action: LoadClients");

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
        public async Task<IActionResult> Create(CreateClientViewModel model)
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

                    _logger.LogError($"Exception thrown while creating a client company. Exception Message: {errorModel.ErrorMessage}");

                    return View("Error", errorModel);
                }

                return RedirectToAction("List");
            }

            return View(model);
        }

        [Authorize(Roles = "Super Admin")]
        [HttpGet]
        public async Task<IActionResult> Edit(string clientId)
        {
            var modelResult = await _service.GetClientDetails(clientId);

            if (modelResult.IsFailed)
            {
                var errorModel = new ErrorModel
                {
                    ErrorMessage = modelResult.ErrorMessages?.FirstOrDefault()
                };

                _logger.LogError($"Exception thrown while reading client company details. Exception Message: {errorModel.ErrorMessage}");

                return View("Error", errorModel);
            }

            return View(modelResult.Data);
        }

        [Authorize(Roles = "Super Admin")]
        [HttpPost]
        public async Task<IActionResult> Edit(EditClientViewModel model)
        {
            if (ModelState.IsValid)
            {
                var updateResult = await _service.Update(model);
                if (updateResult.IsFailed)
                {
                    var errorModel = new ErrorModel
                    {
                        ErrorMessage = updateResult.ErrorMessages?.FirstOrDefault()
                    };

                    _logger.LogError($"Exception thrown while editing a client company. Exception Message: {errorModel.ErrorMessage}");

                    return View("Error", errorModel);
                }

                return RedirectToAction("List");
            }

            return View(model);
        }
    }
}
