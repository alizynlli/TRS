using System.ComponentModel.DataAnnotations;

namespace TRS.Web.ViewModels
{
    public class CreateClientViewModel
    {
        [Required(ErrorMessage = "Müştəri adı boş ola bilməz!")]
        [MaxLength(100, ErrorMessage = "Müştəri adı 100 simvoldan çox ola bilməz!")]
        [Display(Name = "Müştəri adı")]
        public string ClientName { get; set; }
        [Display(Name = "Ünvanı")]
        public string Address { get; set; }
    }
}
