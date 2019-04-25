using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Data.Entity;

namespace Trasy.Models
{
    public class DataContext : DbContext
    {
        public DbSet<Trasy.Models.Trasa> trasa { get; set; }

        public DbSet<Trasy.Models.Bod> bod { get; set; }


        public DataContext() : base("name=DataContext") //odvolavam sa na connection String
        {

        }

        protected override void OnModelCreating(DbModelBuilder modelBuilder) //aby znovu nevytvaralo dbs
        {

        }

    }
}