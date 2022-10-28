using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;

namespace TrafficSignsApi.Controllers
{
    [Route("[controller]")]
    [ApiController]
    public class LabelsController : ControllerBase
    {
        private readonly ILogger<DetectImageController> _logger;
        private readonly IConfiguration _configuration;

        private const char _tableColumnsDivider = ',';

        public LabelsController(ILogger<DetectImageController> logger, IConfiguration configuration)
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
