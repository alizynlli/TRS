using System.ComponentModel.DataAnnotations;

namespace TRS.Web.ViewModels.ClientTaskType
{
    public class EditClientTaskTypeViewModel
    {
        [Required(ErrorMessage = "Id boş ola bilməz!")]
        public string Id { get; set; }

        [Required(ErrorMessage = "Tapşırıq tipi boş ola bilməz!")]
        [Display(Name = "Tapşırıq tipi")]
        public string TypeName { get; set; }
    }
}
