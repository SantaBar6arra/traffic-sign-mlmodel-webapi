using Data.Context;
using Data.Repositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Data
{
    public class UnitOfWork : IUnitOfWork
    {
        private readonly TrafficSignsContext _context;
        public PredictionFeedbackRepository PredictionFeedbackRepository { get; private set; }

        public UnitOfWork(string connectionString)
        {
            _context = new(connectionString);
            PredictionFeedbackRepository = new(_context);
        }

        public int Done()
        {
            return _context.SaveChanges();
        }
    }
}
