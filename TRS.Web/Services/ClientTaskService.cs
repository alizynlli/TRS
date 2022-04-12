using System;
using TRS.Data.Repositories.Abstract;

namespace TRS.Web.Services
{
    public class ClientTaskService : IDisposable
    {
        private readonly IUnitOfWork _unitOfWork;

        public ClientTaskService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public void Dispose()
        {
            _unitOfWork?.Dispose();
        }
    }
}
