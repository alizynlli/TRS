using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.Rendering;
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
using TRS.Web.ViewModels.ClientUser;

namespace TRS.Web.Services
{
    public class ClientUserService : IDisposable
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;

        public ClientUserService(IUnitOfWork unitOfWork, UserManager<ApplicationUser> userManager, RoleManager<IdentityRole> roleManager)
        {
            _unitOfWork = unitOfWork;
            _userManager = userManager;
            _roleManager = roleManager;
        }

        public void Dispose()
        {
            _unitOfWork?.Dispose();
        }

        public async Task<ActionResult<CreateClientUserViewModel>> GetNewClientUser()
        {
            try
            {
                var clientsResult = await _unitOfWork.ClientRepo.GetAllAsync();

                if (clientsResult.IsFailed)
                    return ActionResult<CreateClientUserViewModel>.Failed($"Müştəri məlumatları gətirilərkən xəta yarandı. Xəta mesajı: {clientsResult.ErrorMessages.FirstOrDefault()}");

                if (clientsResult.Data == null || !clientsResult.Data.Any())
                    return ActionResult<CreateClientUserViewModel>.Failed($"Müştəri tapılmadı.");

                var model = new CreateClientUserViewModel
                {
                    ClientCompanies = clientsResult.Data.Select(c => new SelectListItem { Value = c.Id.ToString(), Text = c.ClientName }).ToList()
                };

                return ActionResult<CreateClientUserViewModel>.Succeed(model);
            }
            catch (Exception e)
            {
                return ActionResult<CreateClientUserViewModel>.Failed(e);
            }
        }

        public async Task<ActionResult> CreateAsync(CreateClientUserViewModel model)
        {
            try
            {
                var role = await _roleManager.FindByNameAsync("Client");

                if (role == null)
                    return ActionResult.Failed("Client role tapılmadı. Cari əməliyyatın icrası üçün client role yaradılmalıdır.");

                var user = new ApplicationUser
                {
                    FirstName = model.FirstName,
                    LastName = model.LastName,
                    UserName = model.UserName,
                    Email = model.Email,
                    ClientId = Guid.Parse(model.ClientCompanyId)
                };

                var createResult = await _userManager.CreateAsync(user, model.Password);

                if (!createResult.Succeeded)
                    return ActionResult.Failed($"İstifadəçi yaradılarkən xəta yarandı. \nXəta mesajı: \n{string.Join('\n', createResult.Errors)}");

                var addToRoleResult = await _userManager.AddToRoleAsync(user, "Client");

                if (!addToRoleResult.Succeeded)
                    return ActionResult.Failed($"İstifadəçi client role-a əlavə edilərkən xəta yarandı. \nXəta mesajı: \n{string.Join('\n', createResult.Errors)}");

                return ActionResult.Succeed();
            }
            catch (Exception e)
            {
                return ActionResult.Failed(e);
            }
        }

        public async Task<ActionResult<DataTableModel<ClientUserLineViewModel>>> GetClientUsers(string draw, string sortColumn, string sortColumnDirection,
            string searchValue, int skip, int pageSize)
        {
            try
            {
                var clientUsersResult = await _unitOfWork.ClientRepo.GetClientUsersAsync();

                if (clientUsersResult.IsFailed)
                    return ActionResult<DataTableModel<ClientUserLineViewModel>>.Failed($"İstifadəçi məlumatları gətirilərkən xəta yarandı. Xəta mesajı: {clientUsersResult.ErrorMessages.FirstOrDefault()}");

                var clientUsersData = from user in clientUsersResult.Data
                                      select new ClientUserLineViewModel
                                      {
                                          Id = user.Id,
                                          FirstName = user.FirstName,
                                          LastName = user.LastName,
                                          UserName = user.UserName,
                                          ClientName = user.Client?.ClientName
                                      };

                if (!(string.IsNullOrEmpty(sortColumn) && string.IsNullOrEmpty(sortColumnDirection)))
                {
                    //ClientData = ClientData.OrderBy(sortColumn + " " + sortColumnDirection);
                }

                if (!string.IsNullOrEmpty(searchValue))
                {
                    clientUsersData = clientUsersData.Where(m => m.FirstName.Contains(searchValue)
                                                       || m.LastName.Contains(searchValue)
                                                       || m.UserName.Contains(searchValue)
                                                       || m.ClientName.Contains(searchValue));
                }

                int recordsTotal = clientUsersData.Count();
                var data = clientUsersData.Skip(skip).Take(pageSize).ToList();

                var model = new DataTableModel<ClientUserLineViewModel>
                {
                    Draw = draw,
                    RecordsFiltered = recordsTotal,
                    RecordsTotal = recordsTotal,
                    Data = data
                };

                return ActionResult<DataTableModel<ClientUserLineViewModel>>.Succeed(model);
            }
            catch (Exception e)
            {
                return ActionResult<DataTableModel<ClientUserLineViewModel>>.Failed(e);
            }
        }

        public async Task<ActionResult> ConfirmTask(string taskId, string clientUserId)
        {
            try
            {
                var taskResult = await _unitOfWork.ClientTaskRepo.FindByIdAsync(taskId);

                if (taskResult.IsFailed)
                    return ActionResult<CreateClientUserViewModel>.Failed($"Tapşırıq gətirilərkən xəta yarandı. Xəta mesajı: {taskResult.ErrorMessages.FirstOrDefault()}");

                if (taskResult.Data == null)
                    return ActionResult<CreateClientUserViewModel>.Failed($"Tapşırıq tapılmadı.");

                var task = taskResult.Data;

                var creatorUserResult = await _unitOfWork.TaskOperationRepo.GetCreatorUser(taskId);

                if (creatorUserResult.IsFailed)
                    return ActionResult.Failed($"Tapırığı yaradan istifadəçi gətirilərkən xəta yarandı. Xəta mesajı: {creatorUserResult.ErrorMessages.FirstOrDefault()}");

                if (creatorUserResult.Data.Id != clientUserId)
                    return ActionResult<CreateClientUserViewModel>.Failed($"Siz bu əməliyyatı icra edə bilməzsiniz. Tapşırıq sizin tərəfinizdən yaradılmamışdır.");

                task.TaskStatus = ClientTaskStatuses.Confirmed;
                var updateResult = await _unitOfWork.ClientTaskRepo.UpdateAsync(task);

                if (updateResult.IsFailed)
                    return ActionResult.Failed($"Tapşırıq statusu güncəllənərkən xəta yarandı. Xəta mesajı: {taskResult.ErrorMessages.FirstOrDefault()}");

                var personnelResult = await _unitOfWork.TaskOperationRepo.GetPersonnelAssumingTheTask(taskId);

                if (personnelResult.IsFailed)
                    return ActionResult.Failed($"Tapşırığı üstlənən personal gətirilərkən xəta yarandı. \nXəta mesajı: {personnelResult.ErrorMessages?.FirstOrDefault()}");

                if (personnelResult.Data == null)
                    return ActionResult.Failed($"Tapşırığı üstlənən personal tapılmadı.");

                var taskOperation = new TaskOperation
                {
                    ClientTaskId = task.Id,
                    TaskOperationType = TaskOperationTypes.Confirmed,
                    UserId = clientUserId
                };

                var result = await _unitOfWork.TaskOperationRepo.AddAsync(taskOperation);

                if (result.IsFailed)
                    return ActionResult.Failed($"Yeni tapşırıq əməliyyatı saxlanarkən xəta yarandı. \nXəta mesajı: {personnelResult.ErrorMessages?.FirstOrDefault()}");

                return ActionResult.Succeed();
            }
            catch (Exception e)
            {
                return ActionResult.Failed(e);
            }
        }

        public async Task<ActionResult<DataTableModel<ClientTaskLineViewModel>>> GetClientTasks(string draw, string sortColumn, string sortColumnDirection,
            string searchValue, int skip, int pageSize, string userId, DateTime firstDate)
        {
            try
            {
                var clientTasksResult = await _unitOfWork.ClientTaskRepo.GetCurrentClientTasksAsync(userId, firstDate);

                if (clientTasksResult.IsFailed)
                {
                    return ActionResult<DataTableModel<ClientTaskLineViewModel>>.Failed($"Tapşırıqlar gətirilərkən xəta yarandı. Xəta mesajı: {clientTasksResult.ErrorMessages?.FirstOrDefault()}");
                }

                var clientTaskData = from tempTask in clientTasksResult.Data
                                     orderby tempTask.CreateDate descending
                                     select new ClientTaskLineViewModel
                                     {
                                         Id = tempTask.Id.ToString(),
                                         Name = tempTask.Name,
                                         TaskStatusInt = (byte)tempTask.TaskStatus,
                                         TaskStatus = tempTask.TaskStatus.DescriptionAttr(),
                                         ImportanceDegree = tempTask.ImportanceDegree.DescriptionAttr(),
                                         TaskType = tempTask.ClientTaskType.Name
                                     };

                if (!(string.IsNullOrEmpty(sortColumn) && string.IsNullOrEmpty(sortColumnDirection)))
                {
                    //ClientData = ClientData.OrderBy(sortColumn + " " + sortColumnDirection);
                }
                if (!string.IsNullOrEmpty(searchValue))
                {
                    clientTaskData = clientTaskData.Where(m => m.Name.Contains(searchValue)
                                                               || m.TaskStatus.Contains(searchValue)
                                                               || m.TaskType.Contains(searchValue)
                                                               || m.ImportanceDegree.Contains(searchValue));
                }

                int recordsTotal = clientTaskData.Count();
                var data = clientTaskData.Skip(skip).Take(pageSize).ToList();

                var model = new DataTableModel<ClientTaskLineViewModel>
                {
                    Draw = draw,
                    RecordsFiltered = recordsTotal,
                    RecordsTotal = recordsTotal,
                    Data = data
                };

                return ActionResult<DataTableModel<ClientTaskLineViewModel>>.Succeed(model);
            }
            catch (Exception e)
            {
                return ActionResult<DataTableModel<ClientTaskLineViewModel>>.Failed(e);
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

        public async Task<ActionResult<CreateClientTaskViewModel>> GetNewTaskModelAsync()
        {
            try
            {
                var taskTypesResult = await _unitOfWork.ClientTaskTypeRepo.GetAllAsync();

                if (taskTypesResult.IsFailed)
                    return ActionResult<CreateClientTaskViewModel>.Failed($"Tapşırıq tipləri gətirilərkən xəta yarandı. Xəta mesajı: {taskTypesResult.ErrorMessages?.FirstOrDefault()}");

                if (taskTypesResult.Data == null || !taskTypesResult.Data.Any())
                    return ActionResult<CreateClientTaskViewModel>.Failed($"Sistemdə Tapşırıq tipi yoxdur. Bu səbəbdən tapşırıq yarada bilməzsiniz.");

                var model = new CreateClientTaskViewModel();
                if (model.ClientTaskTypes == null) model.ClientTaskTypes = new List<SelectListItem>();

                foreach (var taskType in taskTypesResult.Data)
                {
                    model.ClientTaskTypes.Add(new SelectListItem(taskType.Name, taskType.Id.ToString()));
                }

                return ActionResult<CreateClientTaskViewModel>.Succeed(model);
            }
            catch (Exception e)
            {
                return ActionResult<CreateClientTaskViewModel>.Failed(e);
            }
        }

        public async Task<ActionResult<EditClientTaskViewModel>> GetExistingTaskModelAsync(string taskId, string userId)
        {
            try
            {
                var taskResult = await _unitOfWork.ClientTaskRepo.FindByIdAsync(taskId);

                if (taskResult.IsFailed)
                    return ActionResult<EditClientTaskViewModel>.Failed($"Tapşırıq gətirilərkən xəta yarandı. Xəta mesajı: {taskResult.ErrorMessages?.FirstOrDefault()}");

                if (taskResult.Data == null)
                    return ActionResult<EditClientTaskViewModel>.Failed($"Tapşırıq tapılmadı.");

                var task = taskResult.Data;

                var creatorUserResult = await _unitOfWork.TaskOperationRepo.GetCreatorUser(taskId);
                if (creatorUserResult.IsFailed)
                    return ActionResult<EditClientTaskViewModel>.Failed($"Tapşırığı yaradan istifadəçi gətirilərkən xəta yarandı. Xəta mesajı: {creatorUserResult.ErrorMessages?.FirstOrDefault()}");

                if (creatorUserResult.Data.Id != userId)
                    return ActionResult<EditClientTaskViewModel>.Failed($"Bu tapşırıq sizin tərəfinizdən yaradılmayıb. Siz bu tapşırığı dəyişə bilməzsiniz.");

                if (taskResult.Data.TaskStatus != ClientTaskStatuses.NotSeen)
                    return ActionResult<EditClientTaskViewModel>.Failed($"Tapırıq artıq təhvil alınıb. Bu tapşırığın detalları dəyişilə bilməz");

                var model = new EditClientTaskViewModel()
                {
                    Id = taskId,
                    Name = taskResult.Data.Name,
                    Description = taskResult.Data.Description,
                    ClientTaskTypeId = taskResult.Data.ClientTaskTypeId.ToString(),
                    ImportanceDegree = (byte)taskResult.Data.ImportanceDegree
                };

                var taskTypesResult = await _unitOfWork.ClientTaskTypeRepo.GetAllAsync();

                if (taskTypesResult.IsFailed)
                    return ActionResult<EditClientTaskViewModel>.Failed($"Tapşırıq gətirilərkən xəta yarandı. Xəta mesajı: {taskResult.ErrorMessages?.FirstOrDefault()}");

                if (model.ClientTaskTypes == null) model.ClientTaskTypes = new List<SelectListItem>();

                foreach (var taskType in taskTypesResult.Data)
                {
                    model.ClientTaskTypes.Add(new SelectListItem(taskType.Name, taskType.Id.ToString()));
                }

                return ActionResult<EditClientTaskViewModel>.Succeed(model);
            }
            catch (Exception e)
            {
                return ActionResult<EditClientTaskViewModel>.Failed(e);
            }
        }

        public async Task<ActionResult> CreateClientTaskAsync(CreateClientTaskViewModel model, string clientUserId)
        {
            try
            {
                var clientTask = new ClientTask
                {
                    Name = model.Name,
                    Description = model.Description,
                    CreateDate = DateTime.Now,
                    ClientTaskTypeId = Guid.Parse(model.ClientTaskTypeId),
                    CreatedByClientUser = true,
                    ImportanceDegree = (ImportanceDegrees)model.ImportanceDegree,
                    TaskStatus = ClientTaskStatuses.NotSeen
                };

                var createResult = await _unitOfWork.ClientTaskRepo.AddAsync(clientTask);
                if (createResult.IsFailed)
                    return ActionResult.Failed($"Tapşırıq yaradılarkən xəta yarandı. Xəta mesajı: {createResult.ErrorMessages?.FirstOrDefault()}");

                var taskOperation = new TaskOperation
                {
                    ClientTaskId = clientTask.Id,
                    TaskOperationType = TaskOperationTypes.Created,
                    UserId = clientUserId,
                    OperationDate = DateTime.Now
                };

                var operationCreationResult = await _unitOfWork.TaskOperationRepo.AddAsync(taskOperation);

                if (operationCreationResult.IsFailed)
                    return ActionResult.Failed($"Tapşırıq əməliyyatı saxlanarkən xəta yarandı. Xəta mesajı: {operationCreationResult.ErrorMessages?.FirstOrDefault()}");

                return ActionResult.Succeed();
            }
            catch (Exception e)
            {
                return ActionResult.Failed(e);
            }
        }

        public async Task<ActionResult> EditClientTaskAsync(EditClientTaskViewModel model, string clientUserId)
        {
            try
            {
                var taskResult = await _unitOfWork.ClientTaskRepo.FindByIdAsync(model.Id);

                if (taskResult.IsFailed)
                    return ActionResult<EditClientTaskViewModel>.Failed($"Tapşırıq gətirilərkən xəta yarandı. Xəta mesajı: {taskResult.ErrorMessages?.FirstOrDefault()}");

                if (taskResult.Data == null)
                    return ActionResult<EditClientTaskViewModel>.Failed($"Tapşırıq tapılmadı.");

                var task = taskResult.Data;

                var creatorUserResult = await _unitOfWork.TaskOperationRepo.GetCreatorUser(model.Id);
                if (creatorUserResult.IsFailed)
                    return ActionResult<EditClientTaskViewModel>.Failed($"Tapşırığı yaradan istifadəçi gətirilərkən xəta yarandı. Xəta mesajı: {creatorUserResult.ErrorMessages?.FirstOrDefault()}");

                if (creatorUserResult.Data.Id != clientUserId)
                    return ActionResult<EditClientTaskViewModel>.Failed($"Bu tapşırıq sizin tərəfinizdən yaradılmayıb. Siz bu tapşırığı dəyişə bilməzsiniz.");

                if (taskResult.Data.TaskStatus != ClientTaskStatuses.NotSeen)
                    return ActionResult<EditClientTaskViewModel>.Failed($"Tapırıq artıq təhvil alınıb. Bu tapşırığın detalları dəyişilə bilməz");

                var clientTask = taskResult.Data;

                clientTask.Name = model.Name;
                clientTask.Description = model.Description;
                clientTask.ClientTaskTypeId = Guid.Parse(model.ClientTaskTypeId);
                clientTask.ImportanceDegree = (ImportanceDegrees)model.ImportanceDegree;

                var updateResult = await _unitOfWork.ClientTaskRepo.UpdateAsync(clientTask);

                if (updateResult.IsFailed)
                    return ActionResult.Failed($"Tapşırıq güncəllənərkən xəta yarandı. Xəta mesajı: {updateResult.ErrorMessages?.FirstOrDefault()}");

                return ActionResult.Succeed();
            }
            catch (Exception e)
            {
                return ActionResult.Failed(e);
            }
        }
    }
}
