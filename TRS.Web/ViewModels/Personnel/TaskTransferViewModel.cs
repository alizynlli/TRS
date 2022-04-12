using Microsoft.AspNetCore.Mvc.Rendering;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace TRS.Web.ViewModels.Personnel
{
    public class TaskTransferViewModel
    {
        public string TaskId { get; set; }
        public string TaskName { get; set; }
        [Required(ErrorMessage = "Personal seçilməlidir.")]
        public string PersonnelId { get; set; }
        public List<SelectListItem> PersonnelList { get; set; }
    }
}
