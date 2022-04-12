using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using TRS.Core.Constants.Enums;

namespace TRS.Data.Models
{
    [Table("ClientTasks")]
    public class ClientTask
    {
        public ClientTask()
        {
            TaskOperations = new List<TaskOperation>();

        }

        [Key]
        public Guid Id { get; set; }

        [MaxLength(100)]
        public string Name { get; set; }

        public string Description { get; set; }

        public DateTime CreateDate { get; set; }

        public ClientTaskStatuses TaskStatus { get; set; }
        public ImportanceDegrees ImportanceDegree { get; set; }

        public Guid ClientTaskTypeId { get; set; }
        public ClientTaskType ClientTaskType { get; set; }

        public bool CreatedByClientUser { get; set; }
        public bool CreatedByPersonnel { get; set; }
        public bool CreatedBySuperAdmin { get; set; }

        public List<TaskOperation> TaskOperations { get; set; }
    }
}
