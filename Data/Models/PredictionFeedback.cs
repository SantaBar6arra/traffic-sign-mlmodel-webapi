using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Data.Models
{
    public class PredictionFeedback
    {
        public int Id { get; set; }
        public string Label { get; set; }
        public bool IsTrue { get; set; }
        public string ImageUrl { get; set; }
    }
}
