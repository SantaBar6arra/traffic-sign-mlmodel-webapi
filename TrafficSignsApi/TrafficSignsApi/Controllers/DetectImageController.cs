using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR.Protocol;
using System.Security.AccessControl;
using TrafficSignsApi.Models;
using TrafficSignsApi.Services;

namespace TrafficSignsApi.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class DetectImageController : ControllerBase
    {
        private readonly ILogger<DetectImageController> _logger;
        private readonly IConfiguration _configuration;
        private readonly PythonScriptRunner _pyScriptRunner = new();

        public DetectImageController(ILogger<DetectImageController> logger, IConfiguration configuration)
        {
            _logger = logger;
            _configuration = configuration;
        }

        [HttpGet]
        public IActionResult Get([FromBody]Image image)
        {
            PredictionResult predictionResult = new() 
            { 
                Prediction = _pyScriptRunner.Run(image.Path, _configuration["pythonSettings:outputDataFolder"])
            };

            return new JsonResult(predictionResult);
        }

        [HttpPost]
        public IActionResult Post([FromForm]Image image)
        {
            string path = Path.Combine(_configuration["pythonSettings:inputDataFolder"], image.Path);
            try
            {
                using (Stream stream = new FileStream(path, FileMode.Create))
                {
                    image.File.CopyTo(stream);
                }

                PredictionResult predictionResult = new()
                {
                    Prediction = _pyScriptRunner.Run(path, _configuration["pythonSettings:outputDataFolder"])
                };

                FileInfo file = new(path);
                file.Delete();

                return new JsonResult(predictionResult);
            }
            catch (Exception e)
            {
                _logger.LogError(e.Message, DateTime.Now);
                return Problem();
            }
        }
    }
}