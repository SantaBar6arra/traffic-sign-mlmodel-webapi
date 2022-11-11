using Data.Context;
using Data.Models;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Data.Repositories
{
    public class PredictionFeedbackRepository : Repository<PredictionFeedback>, IRepository<PredictionFeedback>
    {
        public TrafficSignsContext Context { get { return _context; } }
        public PredictionFeedbackRepository(TrafficSignsContext context)
            : base(context) { }

        public async Task<IEnumerable<PredictionFeedback>> GetAll() => 
            await _context.PredictionFeedbacks.ToListAsync();
      
        public bool Add(PredictionFeedback feedback)
        {
            try
            {
                _context.PredictionFeedbacks.Add(feedback);
                return true;
            }
            catch
            {
                return false;
            }
        }

        public float GetPrecisionRate()
        {
            return _context.PredictionFeedbacks.Where(f => f.IsTrue).Count() 
                / (float)_context.PredictionFeedbacks.Count();
        }
    }
}
