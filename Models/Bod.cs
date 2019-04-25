using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Trasy.Models
{
    [Table("bod")]
    public class Bod
    {
        [Key]
        public int id { get; set; }

        public int id_trasy { get; set; }
        public String nazov { get; set; }
        public int poradie { get; set; }
        public Boolean typ { get; set; }
        public Single suradnica_x { get; set; }
        public Single suradnica_y { get; set; }
        public float rozloha { get; set; }
        public Single pocasie { get; set; }
        public Boolean podnik { get; set; }
        public String otvrHodiny { get; set; }

    }
}