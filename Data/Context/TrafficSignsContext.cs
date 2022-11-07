using Data.Models;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Data.Context
{
    public  class TrafficSignsContext : DbContext
    {
        public TrafficSignsContext(string nameOrConnectionString) : 
            base(nameOrConnectionString)
        {

        }

        public DbSet<PredictionFeedback> PredictionFeedbacks { get; set; }
    }
}
