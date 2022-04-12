using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;

namespace TRS.Web.ViewModels.Administration
{
    public class CreateSuperAdminViewModel
    {
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
        [EmailAddress(ErrorMessage = "Email formatı doğru deyil!")]
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
    }
}
