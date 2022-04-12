using Microsoft.AspNetCore.Mvc.Rendering;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using TRS.Core.Constants.Enums;
using TRS.Core.Extensions;

namespace TRS.Web.ViewModels.ClientUser
{
    public class EditClientTaskViewModel
    {
        public EditClientTaskViewModel()
        {
            ImportanceDegreeList = new List<SelectListItem>
            {
                new SelectListItem(ImportanceDegrees.Urgent.DescriptionAttr(), ((byte)ImportanceDegrees.Urgent).ToString()),
                new SelectListItem(ImportanceDegrees.DuringTheDay.DescriptionAttr(), ((byte)ImportanceDegrees.DuringTheDay).ToString()),
                new SelectListItem(ImportanceDegrees.WithinThreeDays.DescriptionAttr(), ((byte)ImportanceDegrees.WithinThreeDays).ToString()),
                new SelectListItem(ImportanceDegrees.DuringTheWeek.DescriptionAttr(), ((byte)ImportanceDegrees.DuringTheWeek).ToString())
            };
        }

        [Required(ErrorMessage = "Id boş ola bilməz!")]
        public string Id { get; set; }

        [Required(ErrorMessage = "Tapşırıq adı boş ola bilməz!")]
        [Display(Name = "Tapşırığın qısa adı")]
        public string Name { get; set; }

        [Display(Name = "Açıqlama")]
        public string Description { get; set; }

        [Required(ErrorMessage = "Tapşırıq tipi boş ola bilməz!")]
        [Display(Name = "Tapşırıq tipi")]
        public string ClientTaskTypeId { get; set; }

        [Required(ErrorMessage = "Vaciblik dərəcəsi boş ola bilməz!")]
        [Display(Name = "Vaciblik dərəcəsi")]
        public byte ImportanceDegree { get; set; }

        public List<SelectListItem> ImportanceDegreeList { get; set; }
        public List<SelectListItem> ClientTaskTypes { get; set; }
    }
}
