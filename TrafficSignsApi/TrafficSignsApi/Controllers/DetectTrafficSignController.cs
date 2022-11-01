using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR.Protocol;
using Newtonsoft.Json.Linq;
using System.IO;
using System.Net;
using System.Reflection.Emit;
using System.Security.AccessControl;
using System.Text.RegularExpressions;
using TrafficSignsApi.Models;
using TrafficSignsApi.Services;

using static System.Net.WebRequestMethods;

namespace TrafficSignsApi.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class DetectTrafficSignController : ControllerBase
    {
        private readonly ILogger<DetectTrafficSignController> _logger;
        private readonly IConfiguration _configuration;
        private readonly PythonScriptRunner _pyScriptRunner;

        private const string _outputLabelRegexPattern = @"(?=(?:|;))(.*?):";
        private const string _outputValueRegexPattern = @"\: (.*?)\;";
        private const string _imageOutputDivider = "Image";

        public DetectTrafficSignController(ILogger<DetectTrafficSignController> logger, IConfiguration configuration)
        {
            _logger = logger;
            _configuration = configuration;
            _pyScriptRunner = new();
        }

        [HttpPost]
        [DisableRequestSizeLimit]
        [RequestFormLimits(ValueLengthLimit = int.MaxValue, MultipartBodyLengthLimit = int.MaxValue)]
        public IActionResult Post()
        {
            try
            {
                string guid = Guid.NewGuid().ToString();
                DirectoryInfo photosDirectory = Directory.CreateDirectory(
                    Path.Combine(_configuration[Constants.Constants.InputDataFolder], guid));

                foreach (var file in Request.Form.Files)
                {
                    string path = Path.Combine(photosDirectory.FullName, file.FileName);
                    using (Stream stream = new FileStream(path, FileMode.Create))
                    {
                        file.CopyTo(stream);
                        stream.Close();
                    }
                }

                var predictionResult =
                        FormResults(_pyScriptRunner.Run(
                            photosDirectory.FullName, 
                            _configuration[Constants.Constants.OutputDataFolder]), photosDirectory.FullName);

                photosDirectory.Delete(recursive: true);

                return new JsonResult(predictionResult);
            }
            catch (Exception e)
            {
                _logger.LogError(e.Message, DateTime.Now);
                return Problem(e.Message);
            }
        }

        [NonAction]
        private List<PredictionResult> FormResults(string scriptOutput, string inputFolderName)
        {
            List<PredictionResult> results = new();
            var parts = scriptOutput.Split(_imageOutputDivider);

            var inputDirFiles = Directory.GetFiles(inputFolderName);
            int inputPhotoIndex = 0;

            foreach(string part in parts)
            {
                if (string.IsNullOrEmpty(part))
                    continue;

                List<string> labels = GetMatchGroups(part, _outputLabelRegexPattern);
                List<string> values = GetMatchGroups(part, _outputValueRegexPattern);

                results.Add(new()
                {
                    FileName = Path.GetFileName(inputDirFiles[inputPhotoIndex]),
                    PredictionData = GetPredictionDataFromImage(labels, values)
                });

                inputPhotoIndex++;
            }
            return results;
        }

        [NonAction]
        private List<PredictionData> GetPredictionDataFromImage(List<string> labels, List<string> values)
        {
            List<PredictionData> predictionData = new();

            for (int j = 0; j < labels.Count && j < values.Count; j++)
                predictionData.Add(new()
                {
                    Label = labels[j],
                    Value = values[j]
                });

            return predictionData;
        }

        // better rename 
        [NonAction]
        private List<string> GetMatchGroups(string scriptOutput, string pattern)
        {
            var matches = Regex.Matches(scriptOutput, pattern);   
            List<string> matchGroups = new();

            for (int i = 0; i < matches.Count; i++)
                matchGroups.Add(matches[i].Groups[1].Value);

            return matchGroups;
        }
    }
}