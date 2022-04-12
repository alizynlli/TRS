using System.ComponentModel.DataAnnotations;

namespace TRS.Web.ViewModels.Account
{
    public class LoginViewModel
    {
        [Required(ErrorMessage = "İstifadəçi adı daxil edilməyib!")]
        [Display(Name ="İstifadəçi adı")]
        public string UserName { get; set; }

        [Required(ErrorMessage = "Şifrə daxil edilməyib!")]
        [Display(Name = "Şifrə")]
        [DataType(DataType.Password)]
        public string Password { get; set; }
    }
}
