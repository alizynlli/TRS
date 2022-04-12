using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TRS.Core.Extensions;
using TRS.Core.Helpers;
using TRS.Data.Repositories.Abstract;
using TRS.Web.Models;
using TRS.Web.ViewModels.Administration.ClientTask;

namespace TRS.Web.Services
{
    public class AdministrationService : IDisposable
    {
        private readonly IUnitOfWork _unitOfWork;

        public AdministrationService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public void Dispose() => _unitOfWork?.Dispose();

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

                var taskData = from task in taskResult.Data
                               select new NewTaskLineViewModel
                               {
                                   Id = task.Id.ToString(),
                                   TaskName = task.Name,
                                   TaskType = task.ClientTaskType.Name,
                                   ImportanceDegree = task.ImportanceDegree.DescriptionAttr()
                               };

                if (!(string.IsNullOrEmpty(sortColumn) && string.IsNullOrEmpty(sortColumnDirection)))
                {
                    //ClientData = ClientData.OrderBy(sortColumn + " " + sortColumnDirection);
                }

                if (!string.IsNullOrEmpty(searchValue))
                {
                    taskData = taskData.Where(u => u.TaskName.Contains(searchValue)
                                                   || u.TaskType.Contains(searchValue)
                                                   || u.ImportanceDegree.Contains(searchValue))
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
            string searchValue, int skip, int pageSize, DateTime firstDate)
        {
            try
            {
                var taskResult = await _unitOfWork.ClientTaskRepo.GetUnderConsiderationTasksAsync(firstDate);

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
                        TaskType = task.ClientTaskType.Name,
                        ImportanceDegree = task.ImportanceDegree.DescriptionAttr()
                    };

                    taskData.Add(line);
                }

                if (!(string.IsNullOrEmpty(sortColumn) && string.IsNullOrEmpty(sortColumnDirection)))
                {
                    //ClientData = ClientData.OrderBy(sortColumn + " " + sortColumnDirection);
                }

                if (!string.IsNullOrEmpty(searchValue))
                {
                    taskData = taskData.Where(u => u.TaskName.Contains(searchValue)
                                                   || u.TaskType.Contains(searchValue)
                                                   || u.ImportanceDegree.Contains(searchValue))
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
            string searchValue, int skip, int pageSize, DateTime firstDate)
        {
            try
            {
                var taskResult = await _unitOfWork.ClientTaskRepo.GetCompletedTasksAsync(firstDate);

                if (taskResult.IsFailed)
                {
                    return ActionResult<DataTableModel<CompletedTaskLineViewModel>>
                        .Failed($"Tapşırıqlar gətirilərkən xəta yarandı. Xəta mesajı: {taskResult.ErrorMessages?.FirstOrDefault()}");
                }

                var taskData = from task in taskResult.Data
                               select new CompletedTaskLineViewModel
                               {
                                   Id = task.Id.ToString(),
                                   TaskName = task.Name,
                                   TaskType = task.ClientTaskType.Name,
                                   ImportanceDegree = task.ImportanceDegree.DescriptionAttr()
                               };

                if (!(string.IsNullOrEmpty(sortColumn) && string.IsNullOrEmpty(sortColumnDirection)))
                {
                    //ClientData = ClientData.OrderBy(sortColumn + " " + sortColumnDirection);
                }

                if (!string.IsNullOrEmpty(searchValue))
                {
                    taskData = taskData.Where(u => u.TaskName.Contains(searchValue)
                                                   || u.TaskType.Contains(searchValue)
                                                   || u.ImportanceDegree.Contains(searchValue))
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
                    TaskName = task.Name,
                    Description = task.Description,
                    TaskType = task.ClientTaskType.Name,
                    TaskStatus = task.TaskStatus.DescriptionAttr(),
                    TaskStatusConst = (byte)task.TaskStatus,
                    ImportanceDegree = task.ImportanceDegree.DescriptionAttr()
                };

                foreach (var operation in task.TaskOperations)
                {
                    var line = new TaskOperationViewModel
                    {
                        UserName = $"{operation.User.FirstName} {operation.User.LastName}",
                        CompanyName = operation.User.Client?.ClientName,
                        OperationDate = operation.OperationDate.ToString("yyyy-MM-dd HH:mm"),
                        TaskOperationTypeConst = operation.TaskOperationType
                    };

                    model.Operations.Add(line);
                }

                return ActionResult<TaskDetailsViewModel>.Succeed(model);
            }
            catch (Exception e)
            {
                return ActionResult<TaskDetailsViewModel>.Failed(e);
            }
        }
    }
}
