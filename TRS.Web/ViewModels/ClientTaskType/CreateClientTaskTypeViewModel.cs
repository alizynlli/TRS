using System.ComponentModel.DataAnnotations;

namespace TRS.Web.ViewModels.ClientTaskType
{
    public class CreateClientTaskTypeViewModel
    {
        [Required(ErrorMessage = "Tapşırıq tipi boş ola bilməz!")]
        [Display(Name = "Tapşırıq tipi")]
        public string TypeName { get; set; }
    }
}
