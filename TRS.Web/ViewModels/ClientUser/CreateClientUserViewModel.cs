using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace TRS.Web.ViewModels.ClientUser
{
    public class CreateClientUserViewModel
    {
        public CreateClientUserViewModel()
        {
            ClientCompanies = new List<SelectListItem>();
        }

        [Required(ErrorMessage = "Ad daxil edilməyib!")]
        [Display(Name = "Ad")]
        public string FirstName { get; set; }

        [Required(ErrorMessage = "Soyad daxil edilməyib!")]
        [Display(Name = "Soyad")]
        public string LastName { get; set; }

        [Required(ErrorMessage = "İstifadəçi adı daxil edilməyib!")]
        [Display(Name = "İstifadəçi adı")]
        [Remote("IsUserNameInUse", "Account")]
        public string UserName { get; set; }

        [Display(Name = "Email")]
        [EmailAddress]
        public string Email { get; set; }

        [Required(ErrorMessage = "Şifrə daxil edilməyib!")]
        [DataType(DataType.Password)]
        [Display(Name = "Şifrə")]
        public string Password { get; set; }

        [Required(ErrorMessage = "Şifrə təkrarı daxil edilməyib!")]
        [DataType(DataType.Password)]
        [Compare("Password", ErrorMessage = "Şifrələr üst-üstə düşmür.")]
        [Display(Name = "Şifrə təkrarı")]
        public string ConfirmPassword { get; set; }

        [Required(ErrorMessage = "Şirkət daxil edilməyib!")]
        [Display(Name = "Şirkət")]
        public string ClientCompanyId { get; set; }

        public List<SelectListItem> ClientCompanies { get; set; }
    }
}
