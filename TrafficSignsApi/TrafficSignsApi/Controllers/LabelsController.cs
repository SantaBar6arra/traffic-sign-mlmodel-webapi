using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;

namespace TrafficSignsApi.Controllers
{
    [Route("[controller]")]
    [ApiController]
    public class LabelsController : ControllerBase
    {
        private readonly ILogger<LabelsController> _logger;
        private readonly IConfiguration _configuration;

        private const char _tableColumnsDivider = ',';

        public LabelsController(ILogger<LabelsController> logger, IConfiguration configuration)
        {
            _logger = logger;
            _configuration = configuration;
        }

        [HttpGet]
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
