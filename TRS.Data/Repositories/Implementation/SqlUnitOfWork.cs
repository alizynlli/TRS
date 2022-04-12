using TRS.Data.Repositories.Abstract;

namespace TRS.Data.Repositories.Implementation
{
    public class SqlUnitOfWork : IUnitOfWork
    {
        private readonly ApplicationDbContext _dbContext;

        public SqlUnitOfWork(ApplicationDbContext dbContext)
        {
            _dbContext = dbContext;
            ClientTaskRepo = new SqlClientTaskRepository(dbContext);
            ClientRepo = new SqlClientRepository(dbContext);
            ClientTaskTypeRepo = new SqlClientTaskTypeRepository(dbContext);
            TaskOperationRepo = new SqlTaskOperationRepository(dbContext);
        }

        public IClientTaskRepository ClientTaskRepo { get; set; }
        public IClientRepository ClientRepo { get; set; }
        public IClientTaskTypeRepository ClientTaskTypeRepo { get; set; }
        public ITaskOperationRepository TaskOperationRepo { get; set; }

        public void Dispose()
        {
            _dbContext?.Dispose();
        }
    }
}
