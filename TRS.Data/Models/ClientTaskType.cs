using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TRS.Data.Models
{
    [Table("ClientTaskTypes")]
    public class ClientTaskType
    {
        public ClientTaskType()
        {
            Tasks = new List<ClientTask>();
        }

        [Key]
        public Guid Id { get; set; }

        [MaxLength(100)]
        public string Name { get; set; }

        public List<ClientTask> Tasks { get; set; }
    }
}
