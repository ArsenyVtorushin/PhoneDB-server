using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Server.Models
{
    [Table("companies", Schema = "public")]
    public class Company
    {
        [Key]
        [Column("id")]
        public int Id { get; set; }

        [Column("title")]
        [Required]
        public string Title { get; set; }

        [Column("ceo")]
        public string CEO { get; set; }

        [Column("capital")]
        public double Capital { get; set; }

        public List<Phone> PhoneEntities { get; set; }
    }
}
