using System.ComponentModel.DataAnnotations;

namespace TRS.Web.ViewModels.Administration
{
    public class CreateRoleViewModel
    {
        [Required(ErrorMessage ="Rol adı boş ola bilməz!")]
        public string Name { get; set; }
    }
}
