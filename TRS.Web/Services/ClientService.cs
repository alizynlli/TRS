using System;
using System.Linq;
using System.Threading.Tasks;
using TRS.Core.Helpers;
using TRS.Data.Models;
using TRS.Data.Repositories.Abstract;
using TRS.Web.Models;
using TRS.Web.ViewModels;
using TRS.Web.ViewModels.Client;

namespace TRS.Web.Services
{
    public class ClientService : IDisposable
    {
        private readonly IUnitOfWork _unitOfWork;

        public ClientService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<ActionResult<DataTableModel<ClientLineViewModel>>> GetClients(string draw, string sortColumn, string sortColumnDirection,
            string searchValue, int skip, int pageSize)
        {
            try
            {
                var clientResult = await _unitOfWork.ClientRepo.GetAllAsync();

                if (clientResult.IsFailed)
                {
                    return ActionResult<DataTableModel<ClientLineViewModel>>
                        .Failed($"Müştəri şirkət gətirilərkən xəta yarandı. Xəta mesajı: {clientResult.ErrorMessages?.FirstOrDefault()}");
                }

                var clientData = from tempClient in clientResult.Data
                                 select new ClientLineViewModel
                                 {
                                     Id = tempClient.Id.ToString(),
                                     ClientName = tempClient.ClientName,
                                     Address = tempClient.Address
                                 };

                if (!(string.IsNullOrEmpty(sortColumn) && string.IsNullOrEmpty(sortColumnDirection)))
                {
                    //ClientData = ClientData.OrderBy(sortColumn + " " + sortColumnDirection);
                }

                if (!string.IsNullOrEmpty(searchValue))
                {
                    clientData = clientData.Where(m => m.ClientName.Contains(searchValue) || m.Address.Contains(searchValue));
                }

                int recordsTotal = clientData.Count();
                var data = clientData.Skip(skip).Take(pageSize).ToList();

                var model = new DataTableModel<ClientLineViewModel>
                {
                    Draw = draw,
                    RecordsFiltered = recordsTotal,
                    RecordsTotal = recordsTotal,
                    Data = data
                };

                return ActionResult<DataTableModel<ClientLineViewModel>>.Succeed(model);
            }
            catch (Exception e)
            {
                return ActionResult<DataTableModel<ClientLineViewModel>>.Failed(e);
            }
        }

        public async Task<ActionResult> Create(CreateClientViewModel model)
        {
            try
            {
                var client = new Client
                {
                    ClientName = model.ClientName,
                    Address = model.Address
                };

                var result = await _unitOfWork.ClientRepo.AddAsync(client);

                if (result.IsFailed)
                {
                    return ActionResult.Failed($"Müştəri saxlanarkən xəta yarandı. Xəta mesajı: {result.ErrorMessages?.FirstOrDefault()}");
                }

                return ActionResult.Succeed();
            }
            catch (Exception e)
            {
                return ActionResult.Failed(e);
            }
        }

        public async Task<ActionResult<EditClientViewModel>> GetClientDetails(string clientId)
        {
            try
            {
                var clientResult = await _unitOfWork.ClientRepo.FindByIdAsync(clientId);

                if (clientResult.IsFailed)
                    return ActionResult<EditClientViewModel>.Failed($"Müştəri məlumatları gətirilərkən xəta yarandı. Xəta mesajı: {clientResult.ErrorMessages.FirstOrDefault()}");
                
                if (clientResult.Data == null)
                    return ActionResult<EditClientViewModel>.Failed($"Müştəri tapılmadı.");

                var model = new EditClientViewModel
                {
                    Id = clientId,
                    ClientName = clientResult.Data.ClientName,
                    Address = clientResult.Data.Address
                };

                return ActionResult<EditClientViewModel>.Succeed(model);
            }
            catch (Exception e)
            {
                return ActionResult<EditClientViewModel>.Failed(e);
            }
        }

        public async Task<ActionResult> Update(EditClientViewModel model)
        {
            try
            {
                var clientResult = await _unitOfWork.ClientRepo.FindByIdAsync(model.Id);

                if (clientResult.IsFailed)
                    return ActionResult.Failed($"Müştəri məlumatları gətirilərkən xəta yarandı. Xəta mesajı: {clientResult.ErrorMessages.FirstOrDefault()}");
                
                if (clientResult.Data == null)
                    return ActionResult.Failed($"Müştəri tapılmadı.");

                var client = clientResult.Data;

                client.ClientName = model.ClientName;
                client.Address = model.Address;
                var updateResult = await _unitOfWork.ClientRepo.UpdateAsync(client);
                if (updateResult.IsFailed)
                    return ActionResult.Failed($"Müştəri məlumatları gətirilərkən xəta yarandı. Xəta mesajı: {updateResult.ErrorMessages.FirstOrDefault()}");

                return ActionResult.Succeed();
            }
            catch (Exception e)
            {
                return ActionResult.Failed(e);
            }
        }

        public void Dispose()
        {
            _unitOfWork?.Dispose();
        }
    }
}
