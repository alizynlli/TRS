using Microsoft.AspNetCore.Identity;
using System;
using System.Threading.Tasks;
using TRS.Core.Helpers;
using TRS.Data.Models;
using TRS.Data.Repositories.Abstract;
using TRS.Web.ViewModels.Account;

namespace TRS.Web.Services
{
    public class AccountService : IDisposable
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;

        public AccountService(IUnitOfWork unitOfWork, UserManager<ApplicationUser> userManager, SignInManager<ApplicationUser> signInManager)
        {
            _unitOfWork = unitOfWork;
            _userManager = userManager;
            _signInManager = signInManager;
        }

        public EditUserViewModel GetExistingUser(ApplicationUser user)
        {
            var model = new EditUserViewModel
            {
                Id = user.Id,
                FirstName = user.FirstName,
                LastName = user.LastName,
                UserName = user.UserName,
                Email = user.Email
            };

            return model;
        }

        public async Task<ActionResult> EditAsync(EditUserViewModel model)
        {
            try
            {
                var user = await _userManager.FindByIdAsync(model.Id);

                user.FirstName = model.FirstName;
                user.LastName = model.LastName;
                user.UserName = model.UserName;
                user.Email = model.Email;

                var updateResult = await _userManager.UpdateAsync(user);

                if (!updateResult.Succeeded)
                    return ActionResult.Failed($"İstifadəçi məlumatları güncəllənərkən xəta yarandı. \nXəta mesajı: \n{string.Join('\n', updateResult.Errors)}");

                if (!string.IsNullOrEmpty(model.NewPassword))
                {
                    var passwordChangeResult = await _userManager.ChangePasswordAsync(user, model.CurrentPassword, model.NewPassword);

                    if (!passwordChangeResult.Succeeded)
                        return ActionResult.Failed($"Şifrə dəyişilərkən xəta yarandı. \nXəta mesajı: \n{string.Join('\n', passwordChangeResult.Errors)}");
                }

                await _signInManager.SignOutAsync();
                await _signInManager.SignInAsync(user, false);

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
