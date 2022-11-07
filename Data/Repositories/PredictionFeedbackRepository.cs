using Data.Context;
using Data.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Data.Repositories
{
    public class PredictionFeedbackRepository : Repository<PredictionFeedback>, IRepository<PredictionFeedback>
    {
        public PredictionFeedbackRepository(TrafficSignsContext context)
            : base(context) { }

        public IEnumerable<PredictionFeedback> GetAll()
        {
            return _context.PredictionFeedbacks.ToList();
        }

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

        public TrafficSignsContext Context { get { return _context; } }
    }
}
