using System.Net;

namespace TrafficSigns.Models
{
    public class PredictionResult
    {
        public PredictionResult()
        {
            PredictionData = new();
            FileName = string.Empty;
        }
        public string FileName { get; set; }
        public List<PredictionData> PredictionData { get; set; }
    }
}
