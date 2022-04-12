using System.Collections.Generic;
using TRS.Core.Constants.Enums;

namespace TRS.Web.ViewModels.Administration.ClientTask
{
    public class TaskDetailsViewModel
    {
        public TaskDetailsViewModel()
        {
            Operations = new List<TaskOperationViewModel>();
        }

        public string Id { get; set; }
        public string TaskName { get; set; }
        public string Description { get; set; }
        public string TaskType { get; set; }
        public string TaskStatus { get; set; }
        public byte TaskStatusConst { get; set; }
        public string ImportanceDegree { get; set; }

        public List<TaskOperationViewModel> Operations { get; set; }
    }

    public class TaskOperationViewModel
    {
        public TaskOperationTypes TaskOperationTypeConst { get; set; }
        public string UserName { get; set; }
        public string CompanyName { get; set; }
        public string OperationDate { get; set; }
    }
}
