using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Server.Models
{
    [Table("phones", Schema = "public")]
    public class Phone
    {
        [Key]
        [Column("id")]
        public int Id { get; set; }

        [Column("title")]
        [Required]
        [StringLength(50)]
        public string Title { get; set; }

        [Column("companyId")]
        [Required]
        public int CompanyId { get; set; }

        [Column("price")]
        public decimal Price { get; set; }

        public Company CompanyEntity { get; set; }
    }
}
