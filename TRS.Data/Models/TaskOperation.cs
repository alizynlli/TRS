using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using TRS.Core.Constants.Enums;

namespace TRS.Data.Models
{
    [Table("TaskOperations")]
    public class TaskOperation
    {
        public TaskOperation()
        {
            OperationDate = DateTime.Now;
        }

        [Key]
        public Guid Id { get; set; }

        [Required]
        public Guid ClientTaskId { get; set; }
        public ClientTask ClientTask { get; set; }

        public TaskOperationTypes TaskOperationType { get; set; }
        public DateTime OperationDate { get; set; }

        public string UserId { get; set; }
        public ApplicationUser User { get; set; }
    }
}
