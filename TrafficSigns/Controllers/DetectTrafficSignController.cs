using Data;
using Data.Models;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;
using System.Text.RegularExpressions;
using TrafficSigns.Models;
using TrafficSigns.Services;

namespace TrafficSigns.Controllers
{
    public class DetectTrafficSignController : Controller
    {
        private readonly ILogger<DetectTrafficSignController> _logger;
        private readonly IConfiguration _configuration;
        private readonly IUnitOfWork _unitOfWork;
        private readonly PythonScriptRunner _pyScriptRunner;
        private readonly CloudinaryService _cloudinaryService;
        

        private const string _outputLabelRegexPattern = @"(?=(?:|;))(.*?):";
        private const string _outputValueRegexPattern = @"\: (.*?)\;";
        private const string _imageOutputDivider = "Image";
        private const char _tableColumnsDivider = ',';

        public DetectTrafficSignController(ILogger<DetectTrafficSignController> logger, 
            IConfiguration configuration, 
            IUnitOfWork unitOfWork)
        {
            _logger = logger;
            _configuration = configuration;
            _unitOfWork = unitOfWork;
            _pyScriptRunner = new();
            _cloudinaryService = new(configuration);
        }

        [HttpPost("/detect-traffic-sign")]
        [DisableRequestSizeLimit]
        [RequestFormLimits(ValueLengthLimit = int.MaxValue, MultipartBodyLengthLimit = int.MaxValue)]
        public async Task<IActionResult> DetectTrafficSignAsync()
        {
            try
            {
                string guid = Guid.NewGuid().ToString();
                DirectoryInfo photosDirectory = Directory.CreateDirectory(
                    Path.Combine(_configuration[Constants.Constants.Python_InputDataFolder], guid));

                var files = Request.Form.Files;
                foreach (var file in files)
                {
                    string path = Path.Combine(photosDirectory.FullName, file.FileName);
                    using (Stream stream = new FileStream(path, FileMode.Create))
                    {
                        file.CopyTo(stream);
                        stream.Close();
                    }
                }

                var predictionResult =
                        FormResults(await _pyScriptRunner.RunAsync(
                            photosDirectory.FullName), photosDirectory.FullName);

                photosDirectory.Delete(recursive: true);

                return new JsonResult(predictionResult);
            }
            catch (Exception e)
            {
                _logger.LogError(e.Message, DateTime.Now);
                return Problem(e.Message);
            }
        }

        [HttpPost("/feedback")]
        public async Task<IActionResult> HandleFeedback(PredictionFeedback feedback)
        {
            feedback.ImageUrl = (await _cloudinaryService.UploadImage(
                Request.Form.Files[0])).SecureUrl.ToString();   

            if (_unitOfWork.PredictionFeedbackRepository.Add(feedback))
                if(await _unitOfWork.Complete() > 0)
                    return Ok();

            return Problem("cannot add feedback to database", statusCode: 500);            
        }

        [HttpGet("/precision-rate")]
        public IActionResult GetPrecisionRate()
        {
            float precisionRate = _unitOfWork.PredictionFeedbackRepository.GetPrecisionRate();
            // mocked, replace soon
            return Ok(precisionRate);
        }

        [NonAction]
        private List<PredictionResult> FormResults(string scriptOutput, string inputFolderName)
        {
            List<PredictionResult> results = new();
            var parts = scriptOutput.Split(_imageOutputDivider);

            var inputDirFiles = Directory.GetFiles(inputFolderName);
            int inputPhotoIndex = 0;

            foreach (string part in parts)
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

        [HttpGet("/labels")]
        public IActionResult GetLabels()
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

        [HttpGet]
        public IActionResult Index()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}