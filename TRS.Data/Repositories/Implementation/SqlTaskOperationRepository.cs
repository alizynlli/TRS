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
    public class SqlTaskOperationRepository : ITaskOperationRepository
    {
        private readonly ApplicationDbContext _dbContext;

        public SqlTaskOperationRepository(ApplicationDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<ActionResult<IEnumerable<TaskOperation>>> GetAllAsync()
        {
            try
            {
                var taskOperations = await _dbContext.TaskOperations
                    .Include(o => o.ClientTask).ThenInclude(t => t.ClientTaskType)
                    .Include(o => o.User).ThenInclude(u => u.Client)
                    .ToListAsync();

                return ActionResult<IEnumerable<TaskOperation>>.Succeed(taskOperations);
            }
            catch (Exception e)
            {
                return ActionResult<IEnumerable<TaskOperation>>.Failed(e);
            }
        }

        public async Task<ActionResult<TaskOperation>> FindByIdAsync(string id)
        {
            try
            {
                var taskOperation = await _dbContext.TaskOperations.FirstOrDefaultAsync(c => c.Id.ToString() == id);
                return ActionResult<TaskOperation>.Succeed(taskOperation);
            }
            catch (Exception e)
            {
                return ActionResult<TaskOperation>.Failed(e);
            }
        }

        public async Task<ActionResult> AddAsync(TaskOperation entity)
        {
            try
            {
                await _dbContext.TaskOperations.AddAsync(entity);
                await _dbContext.SaveChangesAsync();

                return ActionResult.Succeed();
            }
            catch (DbUpdateException)
            {
                return ActionResult.Failed("Bu müştəri adı artıq daxil edilmişdir.");
            }
            catch (Exception e)
            {
                return ActionResult.Failed(e);
            }
        }

        public async Task<ActionResult> UpdateAsync(TaskOperation entity)
        {
            try
            {
                _dbContext.TaskOperations.Update(entity);
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
                var taskOperation = await _dbContext.TaskOperations.FirstOrDefaultAsync(o => o.Id.ToString() == id);
                if (taskOperation != null)
                {
                    _dbContext.TaskOperations.Remove(taskOperation);
                    await _dbContext.SaveChangesAsync();
                }

                return ActionResult.Succeed();
            }
            catch (Exception e)
            {
                return ActionResult.Failed(e);
            }
        }

        public async Task<ActionResult<ApplicationUser>> GetPersonnelAssumingTheTask(string taskId)
        {
            try
            {
                var taskOperation = await _dbContext.TaskOperations.Include(o => o.User)
                    .OrderByDescending(o => o.OperationDate)
                    .FirstOrDefaultAsync(o => o.ClientTaskId.ToString() == taskId && o.TaskOperationType == TaskOperationTypes.WasTaken);

                return ActionResult<ApplicationUser>.Succeed(taskOperation?.User);
            }
            catch (Exception e)
            {
                return ActionResult<ApplicationUser>.Failed(e);
            }
        }

        public async Task<ActionResult<TaskOperation>> GetLastOperation(string taskId, TaskOperationTypes? operationType)
        {
            try
            {
                var taskOperation = await _dbContext.TaskOperations
                    .OrderByDescending(o => o.OperationDate)
                    .FirstOrDefaultAsync(o => o.ClientTaskId.ToString() == taskId && operationType == null ? true : o.TaskOperationType == operationType.Value);

                return ActionResult<TaskOperation>.Succeed(taskOperation);
            }
            catch (Exception e)
            {
                return ActionResult<TaskOperation>.Failed(e);
            }
        }

        public async Task<ActionResult<ApplicationUser>> GetCreatorUser(string taskId)
        {
            try
            {
                var taskOperation = await _dbContext.TaskOperations
                    .Include(o => o.User).ThenInclude(u => u.Client)
                    .FirstOrDefaultAsync(o => o.ClientTaskId.ToString() == taskId && o.TaskOperationType == TaskOperationTypes.Created);

                return ActionResult<ApplicationUser>.Succeed(taskOperation?.User);
            }
            catch (Exception e)
            {
                return ActionResult<ApplicationUser>.Failed(e);
            }
        }
    }
}
