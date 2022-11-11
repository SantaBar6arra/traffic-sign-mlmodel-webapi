using Data.Repositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Data
{
    public interface IUnitOfWork
    {
        public PredictionFeedbackRepository PredictionFeedbackRepository { get; }
        public Task<int> Complete();
    }
}
