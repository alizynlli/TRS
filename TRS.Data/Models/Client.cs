using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TRS.Data.Models
{
    [Table("Clients")]
    public class Client
    {
        [Key]
        public Guid Id { get; set; }

        [MaxLength(100)]
        public string ClientName { get; set; }

        public string Address { get; set; }
    }
}
