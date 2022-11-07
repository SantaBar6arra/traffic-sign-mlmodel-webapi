using Data.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Data.Repositories
{
    public interface IPredictionFeedbackRepository : IRepository<PredictionFeedback>
    {
        public float GetPrecisionRate();
    }
}
