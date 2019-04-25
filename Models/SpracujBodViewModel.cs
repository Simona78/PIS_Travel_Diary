using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Trasy.Models
{
    public class SpracujBodViewModel
    {

        public int id_trasy { get; set; }
        
        public String nazov { get; set; }
        public List<String> mesta { get; set; }
        public List<String> obce { get; set; }
        public Boolean podnik { get; set; }
    }
}