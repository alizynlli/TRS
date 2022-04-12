using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TRS.Core.Constants.Enums;
using TRS.Core.Extensions;
using TRS.Core.Helpers;
using TRS.Data.Models;
using TRS.Data.Repositories.Abstract;
using TRS.Web.Models;
using TRS.Web.ViewModels.Personnel;

namespace TRS.Web.Services
{
    public class PersonnelService : IDisposable
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;

        public PersonnelService(IUnitOfWork unitOfWork, UserManager<ApplicationUser> userManager, RoleManager<IdentityRole> roleManager)
        {
            _unitOfWork = unitOfWork;
            _userManager = userManager;
            _roleManager = roleManager;
        }

        public void Dispose()
        {
            _unitOfWork?.Dispose();
        }

        public async Task<ActionResult<DataTableModel<PersonnelLineViewModel>>> GetPersonnels(string draw, string sortColumn, string sortColumnDirection,
            string searchValue, int skip, int pageSize)
        {
            try
            {
                var personnels = await _userManager.GetUsersInRoleAsync("Personnel");

                var personnelData = from personnel in personnels
                                    select new PersonnelLineViewModel
                                    {
                                        Id = personnel.Id,
                                        FirstName = personnel.FirstName,
                                        LastName = personnel.LastName,
                                        UserName = personnel.UserName,
                                        Email = personnel.Email
                                    };

                if (!(string.IsNullOrEmpty(sortColumn) && string.IsNullOrEmpty(sortColumnDirection)))
                {
                    //ClientData = ClientData.OrderBy(sortColumn + " " + sortColumnDirection);
                }

                if (!string.IsNullOrEmpty(searchValue))
                {
                    personnelData = personnelData.Where(u => u.FirstName.Contains(searchValue)
                                                   || u.LastName.Contains(searchValue)
                                                   || u.UserName.Contains(searchValue)
                                                   || (u.Email?.Contains(searchValue) ?? false))
                        .ToList();
                }

                var recordsTotal = personnelData.Count();
                var data = personnelData.Skip(skip).Take(pageSize).ToList();

                var model = new DataTableModel<PersonnelLineViewModel>
                {
                    Draw = draw,
                    RecordsFiltered = recordsTotal,
                    RecordsTotal = recordsTotal,
                    Data = data
                };

                return ActionResult<DataTableModel<PersonnelLineViewModel>>.Succeed(model);
            }
            catch (Exception e)
            {
                return ActionResult<DataTableModel<PersonnelLineViewModel>>.Failed(e);
            }
        }

        public async Task<ActionResult> Create(CreatePersonnelViewModel model)
        {
            try
            {
                var role = await _roleManager.FindByNameAsync("Personnel");

                if (role == null)
                    return ActionResult.Failed("Personnel role tapılmadı. Cari əməliyyatın icrası üçün personnel role yaradılmalıdır.");

                var user = new ApplicationUser
                {
                    FirstName = model.FirstName,
                    LastName = model.LastName,
                    UserName = model.UserName,
                    Email = model.Email
                };

                var createResult = await _userManager.CreateAsync(user, model.Password);

                if (!createResult.Succeeded)
                    return ActionResult.Failed($"Personal yaradılarkən xəta yarandı. \nXəta mesajı: \n{string.Join('\n', createResult.Errors)}");

                var addToRoleResult = await _userManager.AddToRoleAsync(user, "Personnel");

                if (!addToRoleResult.Succeeded)
                    return ActionResult.Failed($"Personal \"Personnel\" role-a əlavə edilərkən xəta yarandı. \nXəta mesajı: \n{string.Join('\n', createResult.Errors)}");

                return ActionResult.Succeed();
            }
            catch (Exception e)
            {
                return ActionResult.Failed(e);
            }
        }

        public async Task<ActionResult<DataTableModel<NewTaskLineViewModel>>> GetNewTasks(string draw, string sortColumn, string sortColumnDirection,
            string searchValue, int skip, int pageSize, DateTime firstDate)
        {
            try
            {
                var taskResult = await _unitOfWork.ClientTaskRepo.GetNewTasksAsync(firstDate);

                if (taskResult.IsFailed)
                {
                    return ActionResult<DataTableModel<NewTaskLineViewModel>>
                        .Failed($"Tapşırıqlar gətirilərkən xəta yarandı. Xəta mesajı: {taskResult.ErrorMessages?.FirstOrDefault()}");
                }

                var taskData = new List<NewTaskLineViewModel>();

                foreach (var task in taskResult.Data)
                {
                    var line = new NewTaskLineViewModel
                    {
                        Id = task.Id.ToString(),
                        TaskName = task.Name,
                        TaskType = task.ClientTaskType.Name,
                    };

                    var user = task.TaskOperations.FirstOrDefault().User;

                    line.CreatorUserFullName = $"{user.FirstName} {user.LastName}";
                    line.ClientCompanyName = user.Client?.ClientName;

                    taskData.Add(line);
                }
                if (!(string.IsNullOrEmpty(sortColumn) && string.IsNullOrEmpty(sortColumnDirection)))
                {
                    //ClientData = ClientData.OrderBy(sortColumn + " " + sortColumnDirection);
                }

                if (!string.IsNullOrEmpty(searchValue))
                {
                    taskData = taskData.Where(u => u.TaskName.Contains(searchValue)
                                                   || u.CreatorUserFullName.Contains(searchValue)
                                                   || u.ClientCompanyName.Contains(searchValue)
                                                   || u.TaskType.Contains(searchValue))
                        .ToList();
                }

                var recordsTotal = taskData.Count();
                var data = taskData.Skip(skip).Take(pageSize).ToList();

                var model = new DataTableModel<NewTaskLineViewModel>
                {
                    Draw = draw,
                    RecordsFiltered = recordsTotal,
                    RecordsTotal = recordsTotal,
                    Data = data
                };

                return ActionResult<DataTableModel<NewTaskLineViewModel>>.Succeed(model);
            }
            catch (Exception e)
            {
                return ActionResult<DataTableModel<NewTaskLineViewModel>>.Failed(e);
            }
        }

        public async Task<ActionResult<DataTableModel<UnderConsiderationTaskLineViewModel>>> GetUnderConsiderationTasks(string draw, string sortColumn, string sortColumnDirection,
            string searchValue, int skip, int pageSize, string personnelId, DateTime firstDate)
        {
            try
            {
                var taskResult = await _unitOfWork.ClientTaskRepo.GetUnderConsiderationTasksAsync(personnelId, firstDate);

                if (taskResult.IsFailed)
                {
                    return ActionResult<DataTableModel<UnderConsiderationTaskLineViewModel>>
                        .Failed($"Tapşırıqlar gətirilərkən xəta yarandı. Xəta mesajı: {taskResult.ErrorMessages?.FirstOrDefault()}");
                }

                var taskData = new List<UnderConsiderationTaskLineViewModel>();

                foreach (var task in taskResult.Data)
                {
                    var line = new UnderConsiderationTaskLineViewModel
                    {
                        Id = task.Id.ToString(),
                        TaskName = task.Name,
                        TaskType = task.ClientTaskType.Name
                    };

                    var user = task.TaskOperations.FirstOrDefault(o => o.TaskOperationType == TaskOperationTypes.Created).User;

                    line.CreatorUserFullName = $"{user.FirstName} {user.LastName}";
                    line.ClientCompanyName = user.Client?.ClientName;

                    taskData.Add(line);
                }

                if (!(string.IsNullOrEmpty(sortColumn) && string.IsNullOrEmpty(sortColumnDirection)))
                {
                    //ClientData = ClientData.OrderBy(sortColumn + " " + sortColumnDirection);
                }

                if (!string.IsNullOrEmpty(searchValue))
                {
                    taskData = taskData.Where(u => u.TaskName.Contains(searchValue)
                                                   || u.CreatorUserFullName.Contains(searchValue)
                                                   || u.ClientCompanyName.Contains(searchValue)
                                                   || u.TaskType.Contains(searchValue))
                        .ToList();
                }

                var recordsTotal = taskData.Count();
                var data = taskData.Skip(skip).Take(pageSize).ToList();

                var model = new DataTableModel<UnderConsiderationTaskLineViewModel>
                {
                    Draw = draw,
                    RecordsFiltered = recordsTotal,
                    RecordsTotal = recordsTotal,
                    Data = data
                };

                return ActionResult<DataTableModel<UnderConsiderationTaskLineViewModel>>.Succeed(model);
            }
            catch (Exception e)
            {
                return ActionResult<DataTableModel<UnderConsiderationTaskLineViewModel>>.Failed(e);
            }
        }

        public async Task<ActionResult<DataTableModel<CompletedTaskLineViewModel>>> GetCompletedTasks(string draw, string sortColumn, string sortColumnDirection,
            string searchValue, int skip, int pageSize, string personnelId, DateTime firstDate)
        {
            try
            {
                var taskResult = await _unitOfWork.ClientTaskRepo.GetCompletedTasksAsync(personnelId, firstDate);

                if (taskResult.IsFailed)
                {
                    return ActionResult<DataTableModel<CompletedTaskLineViewModel>>
                        .Failed($"Tapşırıqlar gətirilərkən xəta yarandı. Xəta mesajı: {taskResult.ErrorMessages?.FirstOrDefault()}");
                }

                var taskData = new List<CompletedTaskLineViewModel>();

                foreach (var task in taskResult.Data)
                {
                    var line = new CompletedTaskLineViewModel
                    {
                        Id = task.Id.ToString(),
                        TaskName = task.Name,
                        TaskType = task.ClientTaskType.Name
                    };

                    var user = task.TaskOperations.FirstOrDefault(o => o.TaskOperationType == TaskOperationTypes.Created).User;

                    line.CreatorUserFullName = $"{user.FirstName} {user.LastName}";
                    line.ClientCompanyName = user.Client?.ClientName;

                    taskData.Add(line);
                }

                if (!(string.IsNullOrEmpty(sortColumn) && string.IsNullOrEmpty(sortColumnDirection)))
                {
                    //ClientData = ClientData.OrderBy(sortColumn + " " + sortColumnDirection);
                }

                if (!string.IsNullOrEmpty(searchValue))
                {
                    taskData = taskData.Where(u => u.TaskName.Contains(searchValue)
                                                   || u.CreatorUserFullName.Contains(searchValue)
                                                   || (u.ClientCompanyName?.Contains(searchValue) ?? false)
                                                   || u.TaskType.Contains(searchValue))
                        .ToList();
                }

                var recordsTotal = taskData.Count();
                var data = taskData.Skip(skip).Take(pageSize).ToList();

                var model = new DataTableModel<CompletedTaskLineViewModel>
                {
                    Draw = draw,
                    RecordsFiltered = recordsTotal,
                    RecordsTotal = recordsTotal,
                    Data = data
                };

                return ActionResult<DataTableModel<CompletedTaskLineViewModel>>.Succeed(model);
            }
            catch (Exception e)
            {
                return ActionResult<DataTableModel<CompletedTaskLineViewModel>>.Failed(e);
            }
        }

        public async Task<ActionResult> TakeOnTheTask(string taskId, string personnelId)
        {
            try
            {
                var taskResult = await _unitOfWork.ClientTaskRepo.FindByIdAsync(taskId);

                if (taskResult.IsFailed)
                {
                    return ActionResult.Failed($"Tapşırıq gətirilərkən xəta yarandı. Xəta mesajı: {taskResult.ErrorMessages?.FirstOrDefault()}");
                }

                var task = taskResult.Data;

                if (task == null)
                {
                    return ActionResult.Failed($"Tapşırıq tapılmadı.");
                }

                task.TaskStatus = ClientTaskStatuses.UnderConsideration;

                var updateResult = await _unitOfWork.ClientTaskRepo.UpdateAsync(task);

                if (updateResult.IsFailed)
                {
                    return ActionResult.Failed($"Tapşırıq güncəllənərkən xəta yarandı. Xəta mesajı: {updateResult.ErrorMessages?.FirstOrDefault()}");
                }

                var taskOperation = new TaskOperation
                {
                    ClientTaskId = task.Id,
                    TaskOperationType = TaskOperationTypes.WasTaken,
                    UserId = personnelId
                };

                var addResult = await _unitOfWork.TaskOperationRepo.AddAsync(taskOperation);

                if (addResult.IsFailed)
                {
                    return ActionResult.Failed($"Tapşırıq əməliyyatı əlavə edilərkən xəta yarandı. Xəta mesajı: {addResult.ErrorMessages?.FirstOrDefault()}");
                }

                return ActionResult.Succeed();
            }
            catch (Exception e)
            {
                return ActionResult.Failed(e);
            }
        }

        public async Task<ActionResult> CompleteTask(string taskId, string personnelId)
        {
            try
            {
                var taskResult = await _unitOfWork.ClientTaskRepo.FindByIdAsync(taskId);

                if (taskResult.IsFailed)
                {
                    return ActionResult.Failed($"Tapşırıq gətirilərkən xəta yarandı. Xəta mesajı: {taskResult.ErrorMessages?.FirstOrDefault()}");
                }

                var task = taskResult.Data;

                if (task == null)
                {
                    return ActionResult.Failed($"Tapşırıq tapılmadı.");
                }

                task.TaskStatus = ClientTaskStatuses.Completed;

                var updateResult = await _unitOfWork.ClientTaskRepo.UpdateAsync(task);

                if (updateResult.IsFailed)
                {
                    return ActionResult.Failed($"Tapşırıq güncəllənərkən xəta yarandı. Xəta mesajı: {updateResult.ErrorMessages?.FirstOrDefault()}");
                }

                var taskOperation = new TaskOperation
                {
                    ClientTaskId = task.Id,
                    TaskOperationType = TaskOperationTypes.Completed,
                    UserId = personnelId
                };

                var addResult = await _unitOfWork.TaskOperationRepo.AddAsync(taskOperation);

                if (addResult.IsFailed)
                {
                    return ActionResult.Failed($"Tapşırıq əməliyyatı əlavə edilərkən xəta yarandı. Xəta mesajı: {addResult.ErrorMessages?.FirstOrDefault()}");
                }

                return ActionResult.Succeed();
            }
            catch (Exception e)
            {
                return ActionResult.Failed(e);
            }
        }

        public async Task<ActionResult<TaskDetailsViewModel>> GetTaskDetails(string taskId)
        {
            try
            {
                var taskResult = await _unitOfWork.ClientTaskRepo.GetTaskDetails(taskId);

                if (taskResult.IsFailed)
                {
                    return ActionResult<TaskDetailsViewModel>.Failed($"Tapşırıq gətirilərkən xəta yarandı. Xəta mesajı: {taskResult.ErrorMessages?.FirstOrDefault()}");
                }

                var task = taskResult.Data;

                if (task == null)
                {
                    return ActionResult<TaskDetailsViewModel>.Failed($"Tapşırıq tapılmadı.");
                }

                var model = new TaskDetailsViewModel
                {
                    Id = task.Id.ToString(),
                    Name = task.Name,
                    Description = task.Description,
                    TaskType = task.ClientTaskType.Name,
                    TaskStatus = task.TaskStatus.DescriptionAttr(),
                    TaskStatusConst = (byte)task.TaskStatus,
                    ImportanceDegree = task.ImportanceDegree.DescriptionAttr(),
                    CreateDate = task.CreateDate.ToString("yyyy-MM-dd HH:mm")
                };

                var creatorUser = task.TaskOperations.FirstOrDefault(o => o.TaskOperationType == TaskOperationTypes.Created).User;

                model.CreatorUserName = $"{creatorUser.FirstName} {creatorUser.LastName}";
                model.ClientCompany = creatorUser.Client?.ClientName;

                var underConsiderationOperation = task.TaskOperations.OrderByDescending(o => o.OperationDate).FirstOrDefault(o => o.TaskOperationType == TaskOperationTypes.WasTaken);
                if (underConsiderationOperation != null)
                {
                    model.UnderConsiderationDate = underConsiderationOperation.OperationDate.ToString("yyyy-MM-dd HH:mm");
                }

                var completionOperation = task.TaskOperations.FirstOrDefault(o => o.TaskOperationType == TaskOperationTypes.Completed);
                if (completionOperation != null)
                {
                    model.CompletedDate = completionOperation.OperationDate.ToString("yyyy-MM-dd HH:mm");
                }

                var confirmationOperation = task.TaskOperations.FirstOrDefault(o => o.TaskOperationType == TaskOperationTypes.Confirmed);
                if (confirmationOperation != null)
                {
                    model.ConfirmationDate = confirmationOperation.OperationDate.ToString("yyyy-MM-dd HH:mm");
                }

                return ActionResult<TaskDetailsViewModel>.Succeed(model);
            }
            catch (Exception e)
            {
                return ActionResult<TaskDetailsViewModel>.Failed(e);
            }
        }

        public async Task<ActionResult> CreateTaskAsync(CreateTaskViewModel model, string personnelId)
        {
            try
            {
                var clientTask = new ClientTask
                {
                    Name = model.Name,
                    Description = model.Description,
                    CreateDate = DateTime.Now,
                    ClientTaskTypeId = Guid.Parse(model.ClientTaskTypeId),
                    CreatedByPersonnel = true,
                    ImportanceDegree = (ImportanceDegrees)model.ImportanceDegree,
                    TaskStatus = ClientTaskStatuses.NotSeen
                };

                if (model.ITakeItMyself) clientTask.TaskStatus = ClientTaskStatuses.UnderConsideration;

                var createResult = await _unitOfWork.ClientTaskRepo.AddAsync(clientTask);
                if (createResult.IsFailed)
                    return ActionResult.Failed($"Tapşırıq yaradılarkən xəta yarandı. Xəta mesajı: {createResult.ErrorMessages?.FirstOrDefault()}");

                var taskOperation = new TaskOperation
                {
                    ClientTaskId = clientTask.Id,
                    TaskOperationType = TaskOperationTypes.Created,
                    UserId = personnelId,
                    OperationDate = DateTime.Now
                };

                var operationCreationResult = await _unitOfWork.TaskOperationRepo.AddAsync(taskOperation);

                if (operationCreationResult.IsFailed)
                    return ActionResult.Failed($"Tapşırıq əməliyyatı saxlanarkən xəta yarandı. Xəta mesajı: {operationCreationResult.ErrorMessages?.FirstOrDefault()}");

                if (model.ITakeItMyself)
                {
                    var secondOperation = new TaskOperation
                    {
                        ClientTaskId = clientTask.Id,
                        TaskOperationType = TaskOperationTypes.WasTaken,
                        UserId = personnelId,
                        OperationDate = DateTime.Now
                    };

                    var secondOperationResult = await _unitOfWork.TaskOperationRepo.AddAsync(secondOperation);

                    if (secondOperationResult.IsFailed)
                        return ActionResult.Failed($"Tapşırıq əməliyyatı saxlanarkən xəta yarandı. Xəta mesajı: {secondOperationResult.ErrorMessages?.FirstOrDefault()}");
                }

                return ActionResult.Succeed();
            }
            catch (Exception e)
            {
                return ActionResult.Failed(e);
            }
        }

        public async Task<ActionResult<CreateTaskViewModel>> GetNewTaskModelAsync()
        {
            try
            {
                var taskTypesResult = await _unitOfWork.ClientTaskTypeRepo.GetAllAsync();

                if (taskTypesResult.IsFailed)
                    return ActionResult<CreateTaskViewModel>.Failed($"Tapşırıq tipləri gətirilərkən xəta yarandı. Xəta mesajı: {taskTypesResult.ErrorMessages?.FirstOrDefault()}");

                if (taskTypesResult.Data == null || !taskTypesResult.Data.Any())
                    return ActionResult<CreateTaskViewModel>.Failed($"Sistemdə Tapşırıq tipi yoxdur. Bu səbəbdən tapşırıq yarada bilməzsiniz.");

                var model = new CreateTaskViewModel();
                if (model.ClientTaskTypes == null) model.ClientTaskTypes = new List<SelectListItem>();

                foreach (var taskType in taskTypesResult.Data)
                {
                    model.ClientTaskTypes.Add(new SelectListItem(taskType.Name, taskType.Id.ToString()));
                }

                return ActionResult<CreateTaskViewModel>.Succeed(model);
            }
            catch (Exception e)
            {
                return ActionResult<CreateTaskViewModel>.Failed(e);
            }
        }

        public async Task<ActionResult<TaskTransferViewModel>> GetTaskTransferModelAsync(string taskId)
        {
            try
            {
                var taskResult = await _unitOfWork.ClientTaskRepo.FindByIdAsync(taskId);

                if (taskResult.IsFailed) return ActionResult<TaskTransferViewModel>.Failed($"Tapşırıq gətirilərkən xəta yarandı. Xəta mesajı: {taskResult.ErrorMessages?.FirstOrDefault()}");

                if (taskResult.Data == null) return ActionResult<TaskTransferViewModel>.Failed($"Tapşırıq tapılmadı.");

                var model = new TaskTransferViewModel
                {
                    TaskId = taskResult.Data.Id.ToString(),
                    TaskName = taskResult.Data.Name
                };

                if (model.PersonnelList == null) model.PersonnelList = new List<SelectListItem>();

                var users = await _userManager.Users.Where(u => u.ClientId == null).ToListAsync();

                foreach (var user in users)
                {
                    if (await _userManager.IsInRoleAsync(user, "Personnel"))
                    {
                        model.PersonnelList.Add(new SelectListItem($"{user.FirstName} {user.LastName}", user.Id));
                    }
                }

                return ActionResult<TaskTransferViewModel>.Succeed(model);
            }
            catch (Exception e)
            {
                return ActionResult<TaskTransferViewModel>.Failed(e);
            }
        }

        public async Task<ActionResult<TaskTransferViewModel>> TaskTransferAsync(TaskTransferViewModel model, string currentPersonnelId)
        {
            try
            {
                var taskResult = await _unitOfWork.ClientTaskRepo.FindByIdAsync(model.TaskId);

                if (taskResult.IsFailed) return ActionResult<TaskTransferViewModel>.Failed($"Tapşırıq gətirilərkən xəta yarandı. Xəta mesajı: {taskResult.ErrorMessages?.FirstOrDefault()}");

                if (taskResult.Data == null) return ActionResult<TaskTransferViewModel>.Failed($"Tapşırıq tapılmadı.");

                var personnel = await _userManager.FindByIdAsync(model.TaskId);

                var taskOperation = new TaskOperation
                {
                    ClientTaskId = Guid.Parse(model.TaskId),
                    TaskOperationType = TaskOperationTypes.Transferred,
                    UserId = currentPersonnelId
                };

                var operationCreationResult = await _unitOfWork.TaskOperationRepo.AddAsync(taskOperation);
                if (operationCreationResult.IsFailed) return ActionResult<TaskTransferViewModel>.Failed($"Tapşırıq əməliyyatı yaradılarkən xəta yarandı. Xəta mesajı: {operationCreationResult.ErrorMessages?.FirstOrDefault()}");

                var secondOperation = new TaskOperation
                {
                    ClientTaskId = Guid.Parse(model.TaskId),
                    TaskOperationType = TaskOperationTypes.WasTaken,
                    UserId = model.PersonnelId
                };

                var secondOperationCreationResult = await _unitOfWork.TaskOperationRepo.AddAsync(secondOperation);
                if (secondOperationCreationResult.IsFailed) return ActionResult<TaskTransferViewModel>.Failed($"Tapşırıq əməliyyatı yaradılarkən xəta yarandı. Xəta mesajı: {secondOperationCreationResult.ErrorMessages?.FirstOrDefault()}");

                return ActionResult<TaskTransferViewModel>.Succeed(model);
            }
            catch (Exception e)
            {
                return ActionResult<TaskTransferViewModel>.Failed(e);
            }
        }
    }
}
