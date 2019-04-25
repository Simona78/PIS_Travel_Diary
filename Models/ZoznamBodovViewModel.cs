using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Trasy.Models
{
    public class ZoznamBodovViewModel
    {
        public int id { get; set; }
        public List<Bod> b { get; set; }
    }
}