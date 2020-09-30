using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Data.Entity;


namespace Axatel.Models
{
    public class Context : DbContext
    {

        public Context() : base("DefaultConnection")
        {

        }
        public DbSet<Compan> Compans { get; set; }
        public DbSet<BlackList> BlackLists { get; set; }
        public DbSet<Tiket> Tikets { get; set; }
        public DbSet<amoCompan> amoCompans { get; set; }
        public DbSet<amoOperator> amoOperators { get; set; }
        public DbSet<AlfaCompan> alfaCompans { get; set; }




    }


}