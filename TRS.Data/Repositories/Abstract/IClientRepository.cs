using System.Collections.Generic;
using System.Threading.Tasks;
using TRS.Core.Helpers;
using TRS.Data.Models;

namespace TRS.Data.Repositories.Abstract
{
    public interface IClientRepository
    {
        Task<ActionResult<IEnumerable<Client>>> GetAllAsync();
        Task<ActionResult<IEnumerable<ApplicationUser>>> GetClientUsersAsync();
        Task<ActionResult<Client>> FindByIdAsync(string id);
        Task<ActionResult> AddAsync(Client entity);
         Task<ActionResult> UpdateAsync(Client entity);
        Task<ActionResult> DeleteAsync(string id);
    }
}
