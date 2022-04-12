using System.Collections.Generic;
using System.Threading.Tasks;
using TRS.Core.Constants.Enums;
using TRS.Core.Helpers;
using TRS.Data.Models;

namespace TRS.Data.Repositories.Abstract
{
    public interface ITaskOperationRepository
    {
        Task<ActionResult<IEnumerable<TaskOperation>>> GetAllAsync();
        Task<ActionResult<TaskOperation>> FindByIdAsync(string id);
        Task<ActionResult> AddAsync(TaskOperation entity);
        Task<ActionResult> UpdateAsync(TaskOperation entity);
        Task<ActionResult> DeleteAsync(string id);
        Task<ActionResult<ApplicationUser>> GetPersonnelAssumingTheTask(string taskId);
        Task<ActionResult<ApplicationUser>> GetCreatorUser(string taskId);
        Task<ActionResult<TaskOperation>> GetLastOperation(string taskId, TaskOperationTypes? operationType);
    }
}
