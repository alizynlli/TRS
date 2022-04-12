using System;

namespace TRS.Data.Repositories.Abstract
{
    public interface IUnitOfWork : IDisposable
    {
        IClientTaskRepository ClientTaskRepo { get; set; }
        IClientRepository ClientRepo { get; set; }
        IClientTaskTypeRepository ClientTaskTypeRepo { get; set; }
        ITaskOperationRepository TaskOperationRepo { get; set; }

    }
}
