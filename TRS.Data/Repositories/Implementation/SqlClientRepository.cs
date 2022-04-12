using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TRS.Core.Helpers;
using TRS.Data.Models;
using TRS.Data.Repositories.Abstract;

namespace TRS.Data.Repositories.Implementation
{
    public class SqlClientRepository : IClientRepository
    {
        private readonly ApplicationDbContext _dbContext;

        public SqlClientRepository(ApplicationDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<ActionResult<IEnumerable<Client>>> GetAllAsync()
        {
            try
            {
                var clients = await _dbContext.Clients.ToListAsync();
                return ActionResult<IEnumerable<Client>>.Succeed(clients);
            }
            catch (Exception e)
            {
                return ActionResult<IEnumerable<Client>>.Failed(e);
            }
        }

        public async Task<ActionResult<IEnumerable<ApplicationUser>>> GetClientUsersAsync()
        {
            try
            {
                var clientUsers = await _dbContext.Users.Where(user => user.ClientId != null).Include(user => user.Client).ToListAsync();
                return ActionResult<IEnumerable<ApplicationUser>>.Succeed(clientUsers);
            }
            catch (Exception e)
            {
                return ActionResult<IEnumerable<ApplicationUser>>.Failed(e);
            }
        }

        public async Task<ActionResult<Client>> FindByIdAsync(string id)
        {
            try
            {
                var client = await _dbContext.Clients.FirstOrDefaultAsync(c => c.Id.ToString() == id);
                return ActionResult<Client>.Succeed(client);
            }
            catch (Exception e)
            {
                return ActionResult<Client>.Failed(e);
            }
        }

        public async Task<ActionResult> AddAsync(Client entity)
        {
            try
            {
                await _dbContext.Clients.AddAsync(entity);
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

        public async Task<ActionResult> UpdateAsync(Client entity)
        {
            try
            {
                _dbContext.Clients.Update(entity);
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

        public async Task<ActionResult> DeleteAsync(string id)
        {
            try
            {
                var client = await _dbContext.Clients.FirstOrDefaultAsync(c => c.Id.ToString() == id);
                if (client != null)
                {
                    _dbContext.Clients.Remove(client);
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
