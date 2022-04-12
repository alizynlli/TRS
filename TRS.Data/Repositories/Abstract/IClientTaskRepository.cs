using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using TRS.Core.Helpers;
using TRS.Data.Models;

namespace TRS.Data.Repositories.Abstract
{
    public interface IClientTaskRepository
    {
        Task<ActionResult<IEnumerable<ClientTask>>> GetAllAsync();
        Task<ActionResult<IEnumerable<ClientTask>>> GetNewTasksAsync(DateTime firstDate);
        Task<ActionResult<IEnumerable<ClientTask>>> GetUnderConsiderationTasksAsync(DateTime firstDate);
        Task<ActionResult<IEnumerable<ClientTask>>> GetUnderConsiderationTasksAsync(string personnelId, DateTime firstDate);
        Task<ActionResult<IEnumerable<ClientTask>>> GetCompletedTasksAsync(DateTime firstDate);
        Task<ActionResult<IEnumerable<ClientTask>>> GetCompletedTasksAsync(string personnelId, DateTime firstDate);
        Task<ActionResult<IEnumerable<ClientTask>>> GetCurrentClientTasksAsync(string clientUserId, DateTime firstDate);
        Task<ActionResult<ClientTask>> FindByIdAsync(string id);
        Task<ActionResult<ClientTask>> GetTaskDetails(string id);
        Task<ActionResult> AddAsync(ClientTask entity);
        Task<ActionResult> UpdateAsync(ClientTask entity);
        Task<ActionResult> DeleteAsync(string id);
    }
}
