using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TRS.Core.Constants.Enums;
using TRS.Core.Helpers;
using TRS.Data.Models;
using TRS.Data.Repositories.Abstract;

namespace TRS.Data.Repositories.Implementation
{
    public class SqlClientTaskRepository : IClientTaskRepository
    {
        private readonly ApplicationDbContext _dbContext;

        public SqlClientTaskRepository(ApplicationDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<ActionResult<IEnumerable<ClientTask>>> GetAllAsync()
        {
            try
            {
                var data = await _dbContext.ClientTasks.ToListAsync();
                return ActionResult<IEnumerable<ClientTask>>.Succeed(data);
            }
            catch (Exception e)
            {
                return ActionResult<IEnumerable<ClientTask>>.Failed(e);
            }
        }

        public async Task<ActionResult<IEnumerable<ClientTask>>> GetNewTasksAsync(DateTime firstDate)
        {
            try
            {
                var data = await _dbContext.ClientTasks
                    .Where(t => t.TaskStatus == ClientTaskStatuses.NotSeen && t.CreateDate >= firstDate)
                    .Include(t => t.ClientTaskType)
                    .Include(t => t.TaskOperations).ThenInclude(o => o.User).ThenInclude(c => c.Client)
                    .OrderByDescending(t => t.CreateDate)
                    .ToListAsync();

                return ActionResult<IEnumerable<ClientTask>>.Succeed(data);
            }
            catch (Exception e)
            {
                return ActionResult<IEnumerable<ClientTask>>.Failed(e);
            }
        }

        public async Task<ActionResult<IEnumerable<ClientTask>>> GetUnderConsiderationTasksAsync(DateTime firstDate)
        {
            try
            {
                var data = await _dbContext.ClientTasks
                    .Where(t => t.TaskStatus == ClientTaskStatuses.UnderConsideration && t.CreateDate >= firstDate)
                    .Include(t => t.ClientTaskType)
                    .Include(t => t.TaskOperations).ThenInclude(o => o.User).ThenInclude(c => c.Client)
                    .OrderByDescending(t => t.CreateDate)
                    .ToListAsync();

                return ActionResult<IEnumerable<ClientTask>>.Succeed(data);
            }
            catch (Exception e)
            {
                return ActionResult<IEnumerable<ClientTask>>.Failed(e);
            }
        }

        public async Task<ActionResult<IEnumerable<ClientTask>>> GetUnderConsiderationTasksAsync(string personnelId, DateTime firstDate)
        {
            try
            {
                var data = await _dbContext.ClientTasks
                    .Where(t => t.TaskStatus == ClientTaskStatuses.UnderConsideration && t.CreateDate >= firstDate)
                    .Include(t => t.ClientTaskType)
                    .Include(t => t.TaskOperations).ThenInclude(o => o.User).ThenInclude(c => c.Client)
                    .OrderByDescending(t => t.CreateDate)
                    .ToListAsync();

                var resultData = new List<ClientTask>();

                foreach (var task in data)
                {
                    var operation = task.TaskOperations.OrderByDescending(o => o.OperationDate).FirstOrDefault(o => o.TaskOperationType == TaskOperationTypes.WasTaken);

                    if (operation != null && operation.UserId == personnelId)
                    {
                        resultData.Add(task);
                    }
                }

                return ActionResult<IEnumerable<ClientTask>>.Succeed(resultData);
            }
            catch (Exception e)
            {
                return ActionResult<IEnumerable<ClientTask>>.Failed(e);
            }
        }

        public async Task<ActionResult<IEnumerable<ClientTask>>> GetCurrentClientTasksAsync(string clientUserId, DateTime firstDate)
        {
            try
            {
                var data = await _dbContext.ClientTasks
                    .Where(t => t.CreatedByClientUser && t.CreateDate >= firstDate && t.TaskOperations.Any(o => o.UserId == clientUserId && o.TaskOperationType == TaskOperationTypes.Created))
                    .Include(t => t.ClientTaskType)
                    .Include(t => t.TaskOperations)
                        .ThenInclude(o => o.User)
                            .ThenInclude(c => c.Client)
                    .ToListAsync();

                return ActionResult<IEnumerable<ClientTask>>.Succeed(data);
            }
            catch (Exception e)
            {
                return ActionResult<IEnumerable<ClientTask>>.Failed(e);
            }
        }

        public async Task<ActionResult<ClientTask>> FindByIdAsync(string id)
        {
            try
            {
                var clientTask = await _dbContext.ClientTasks.FirstOrDefaultAsync(c => c.Id.ToString() == id);
                return ActionResult<ClientTask>.Succeed(clientTask);
            }
            catch (Exception e)
            {
                return ActionResult<ClientTask>.Failed(e);
            }
        }

        public async Task<ActionResult<ClientTask>> GetTaskDetails(string id)
        {
            try
            {
                var clientTask = await _dbContext.ClientTasks
                    .Include(t => t.ClientTaskType)
                    .Include(t => t.TaskOperations).ThenInclude(o => o.User).ThenInclude(c => c.Client)
                    .OrderBy(o => o.CreateDate)
                    .FirstOrDefaultAsync(c => c.Id.ToString() == id);

                return ActionResult<ClientTask>.Succeed(clientTask);
            }
            catch (Exception e)
            {
                return ActionResult<ClientTask>.Failed(e);
            }
        }

        public async Task<ActionResult> AddAsync(ClientTask entity)
        {
            try
            {
                await _dbContext.ClientTasks.AddAsync(entity);
                await _dbContext.SaveChangesAsync();
                return ActionResult.Succeed();
            }
            catch (Exception e)
            {
                return ActionResult.Failed(e);
            }
        }

        public async Task<ActionResult> UpdateAsync(ClientTask entity)
        {
            try
            {
                _dbContext.ClientTasks.Update(entity);
                await _dbContext.SaveChangesAsync();
                return ActionResult.Succeed();
            }
            catch (Exception e)
            {
                return ActionResult.Failed(e);
            }
        }

        public async Task<ActionResult> DeleteAsync(string id)
        {
            try
            {
                var task = await _dbContext.ClientTasks.FirstOrDefaultAsync(c => c.Id.ToString() == id);
                if (task != null)
                {
                    _dbContext.ClientTasks.Remove(task);
                    await _dbContext.SaveChangesAsync();
                }

                return ActionResult.Succeed();
            }
            catch (Exception e)
            {
                return ActionResult.Failed(e);
            }
        }

        public async Task<ActionResult<IEnumerable<ClientTask>>> GetCompletedTasksAsync(DateTime firstDate)
        {
            try
            {
                var data = await _dbContext.ClientTasks
                    .Where(t => (t.TaskStatus == ClientTaskStatuses.Completed || t.TaskStatus == ClientTaskStatuses.Confirmed) && t.CreateDate >= firstDate)
                    .Include(t => t.ClientTaskType)
                    .Include(t => t.TaskOperations).ThenInclude(o => o.User).ThenInclude(c => c.Client)
                    .OrderByDescending(t => t.CreateDate)
                    .ToListAsync();

                return ActionResult<IEnumerable<ClientTask>>.Succeed(data);
            }
            catch (Exception e)
            {
                return ActionResult<IEnumerable<ClientTask>>.Failed(e);
            }
        }

        public async Task<ActionResult<IEnumerable<ClientTask>>> GetCompletedTasksAsync(string personnelId, DateTime firstDate)
        {
            try
            {
                var data = await _dbContext.ClientTasks
                    .Where(t => (t.TaskStatus == ClientTaskStatuses.Completed || t.TaskStatus == ClientTaskStatuses.Confirmed) && t.CreateDate >= firstDate)
                    .Include(t => t.ClientTaskType)
                    .Include(t => t.TaskOperations).ThenInclude(o => o.User).ThenInclude(c => c.Client)
                    .OrderByDescending(t => t.CreateDate)
                    .ToListAsync();

                //TODO yoxla
                var resultData = new List<ClientTask>();

                foreach (var task in data)
                {
                    var operation = task.TaskOperations.FirstOrDefault(o => o.UserId == personnelId && o.TaskOperationType == TaskOperationTypes.Completed);

                    if (operation != null)
                    {
                        resultData.Add(task);
                    }
                }

                return ActionResult<IEnumerable<ClientTask>>.Succeed(resultData);
            }
            catch (Exception e)
            {
                return ActionResult<IEnumerable<ClientTask>>.Failed(e);
            }
        }
    }
}
