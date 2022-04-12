using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;

namespace TRS.Web.ViewModels.Account
{
    public class EditUserViewModel
    {
        [Required(ErrorMessage ="Id boş ola bilməz")]
        public string Id { get; set; }

        [Required(ErrorMessage = "Ad daxil edilməyib!")]
        [Display(Name = "Ad")]
        public string FirstName { get; set; }

        [Required(ErrorMessage = "Soyad daxil edilməyib!")]
        [Display(Name = "Soyad")]
        public string LastName { get; set; }

        [Required(ErrorMessage = "İstifadəçi adı daxil edilməyib!")]
        [Display(Name = "İstifadəçi adı")]
        public string UserName { get; set; }

        [Display(Name = "Email")]
        [EmailAddress(ErrorMessage = "Email formatı doğru deyil!")]
        public string Email { get; set; }

        [Required(ErrorMessage = "Cari şifrə daxil edilməyib!")]
        [DataType(DataType.Password)]
        [Display(Name = "Cari şifrə")]
        public string CurrentPassword { get; set; }

        [DataType(DataType.Password)]
        [Display(Name = "Yeni şifrə")]
        public string NewPassword { get; set; }

        [DataType(DataType.Password)]
        [Compare("NewPassword", ErrorMessage = "Şifrələr üst-üstə düşmür.")]
        [Display(Name = "Yeni şifrə təkrarı")]
        public string ConfirmPassword { get; set; }
    }
}
