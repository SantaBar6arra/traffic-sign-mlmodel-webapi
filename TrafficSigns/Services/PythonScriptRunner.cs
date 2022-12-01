using Microsoft.Extensions.Configuration;
using System.Diagnostics;
using System.Text;

namespace TrafficSigns.Services
{
    public class PythonScriptRunner
    {
        private readonly string _pythonExeName;
        private readonly string _workingDirectory;
        private readonly string _scriptName;
        private readonly string _modelFolderPath;
        private readonly StringBuilder _queryBuilder = new();

        private readonly IConfiguration _configuration;

        public PythonScriptRunner()
        {
            _configuration = new ConfigurationBuilder().AddJsonFile(Constants.Constants.AppSettingsFileName).Build();
            _pythonExeName = _configuration[Constants.Constants.Python_PythonExeName];
            _workingDirectory = _configuration[Constants.Constants.Python_WorkingDirectory];
            _scriptName = _configuration[Constants.Constants.Python_ScriptName];
            _modelFolderPath = _configuration[Constants.Constants.Python_ModelFolderPath];
        }

        public async Task<string> RunAsync(string imagesFolderPath)
        {
            ProcessStartInfo processStartInfo = new()
            {
                FileName = _pythonExeName,
                UseShellExecute = false,
                CreateNoWindow = true,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                WorkingDirectory = _workingDirectory, 
                Arguments = FormQuery(imagesFolderPath)
            };

            string output = string.Empty, error = String.Empty;

            var process = Process.Start(processStartInfo);
            await process.WaitForExitAsync();
            output = process.StandardOutput.ReadToEnd();
            // error = process.StandardError.ReadToEnd();

            output = output.Substring(output.IndexOf('\t') + 1).Trim('\r', '\n');

            return output;
        }

        private string FormQuery(string imagesFolderPath)
        {
            _queryBuilder.Clear();
            _queryBuilder.Append($"{_scriptName} ");
            _queryBuilder.Append($"-m {_modelFolderPath} ");
            _queryBuilder.Append($"-i {imagesFolderPath} ");

            return _queryBuilder.ToString();
        }
    }
}
