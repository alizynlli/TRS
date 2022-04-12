using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using TRS.Core.Helpers;
using TRS.Data.Models;
using TRS.Data.Repositories.Abstract;

namespace TRS.Data.Repositories.Implementation
{
    public class SqlClientTaskTypeRepository : IClientTaskTypeRepository
    {
        private readonly ApplicationDbContext _dbContext;

        public SqlClientTaskTypeRepository(ApplicationDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<ActionResult<IEnumerable<ClientTaskType>>> GetAllAsync()
        {
            try
            {
                var data = await _dbContext.ClientTaskTypes.ToListAsync();
                return ActionResult<IEnumerable<ClientTaskType>>.Succeed(data);
            }
            catch (Exception e)
            {
                return ActionResult<IEnumerable<ClientTaskType>>.Failed(e);
            }
        }

        public async Task<ActionResult<ClientTaskType>> FindByIdAsync(string id)
        {
            try
            {
                var clientTaskType = await _dbContext.ClientTaskTypes.FirstOrDefaultAsync(c => c.Id.ToString() == id);
                return ActionResult<ClientTaskType>.Succeed(clientTaskType);
            }
            catch (Exception e)
            {
                return ActionResult<ClientTaskType>.Failed(e);
            }
        }

        public async Task<ActionResult> AddAsync(ClientTaskType entity)
        {
            try
            {
                await _dbContext.ClientTaskTypes.AddAsync(entity);
                await _dbContext.SaveChangesAsync();
                return ActionResult.Succeed();
            }
            catch (Exception e)
            {
                return ActionResult.Failed(e);
            }
        }

        public async Task<ActionResult> UpdateAsync(ClientTaskType entity)
        {
            try
            {
                _dbContext.ClientTaskTypes.Update(entity);
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
                var task = await _dbContext.ClientTaskTypes.FirstOrDefaultAsync(c => c.Id.ToString() == id);
                if (task != null)
                {
                    _dbContext.ClientTaskTypes.Remove(task);
                    await _dbContext.SaveChangesAsync();
                }

                return ActionResult.Succeed();
            }
            catch (Exception e)
            {
                return ActionResult.Failed(e);
            }
        }
    }
}
