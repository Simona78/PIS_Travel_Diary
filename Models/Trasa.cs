using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Trasy.Models
{
    [Table("trasa")]
    public class Trasa
    {
        [Key]
        public int id { get; set; }
        public String nazov { get; set; }
    }
}

