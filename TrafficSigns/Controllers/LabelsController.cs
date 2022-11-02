using Microsoft.AspNetCore.Mvc;

namespace TrafficSigns.Controllers
{
    public class LabelsController : Controller
    {
        private readonly ILogger<LabelsController> _logger;
        private readonly IConfiguration _configuration;

        private const char _tableColumnsDivider = ',';

        public LabelsController(ILogger<LabelsController> logger, IConfiguration configuration)
        {
            _logger = logger;
            _configuration = configuration;
        }

        [HttpGet("/labels")]
        public IActionResult Get()
        {
            List<string> labels = new();
            try
            {
                using (var streamReader = new StreamReader(_configuration[Constants.Constants.LabelsFilePath]))
                {
                    while (!streamReader.EndOfStream)
                    {
                        var splits = streamReader.ReadLine().Split(_tableColumnsDivider);
                        labels.Add(splits[1]);
                    }
                    streamReader.Close();
                }
                labels.RemoveAt(0); // deleting column name

                return new JsonResult(labels);
            }
            catch (Exception e)
            {
                _logger.LogError(e.Message, DateTime.Now);
                return Problem();
            }
        }
    }
}
