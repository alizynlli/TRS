using System.Collections.Generic;
using System.Threading.Tasks;
using TRS.Core.Helpers;
using TRS.Data.Models;

namespace TRS.Data.Repositories.Abstract
{
    public interface IClientTaskTypeRepository
    {
        Task<ActionResult<IEnumerable<ClientTaskType>>> GetAllAsync();
        Task<ActionResult<ClientTaskType>> FindByIdAsync(string id);
        Task<ActionResult> AddAsync(ClientTaskType entity);
         Task<ActionResult> UpdateAsync(ClientTaskType entity);
        Task<ActionResult> DeleteAsync(string id);
    }
}
