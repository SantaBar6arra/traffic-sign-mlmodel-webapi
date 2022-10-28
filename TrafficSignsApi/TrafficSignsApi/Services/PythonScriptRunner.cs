using Microsoft.Extensions.Configuration;
using System.Diagnostics;
using System.Text;

namespace TrafficSignsApi.Services
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
            _pythonExeName = _configuration[Constants.Constants.PythonExeName];
            _workingDirectory = _configuration[Constants.Constants.WorkingDirectory];
            _scriptName = _configuration[Constants.Constants.ScriptName];
            _modelFolderPath = _configuration[Constants.Constants.ModelFolderPath];
        }

        public string Run(string imagesFolderPath, string resultsFolderPath)
        {
            ProcessStartInfo processStartInfo = new()
            {
                FileName = _pythonExeName,
                UseShellExecute = false,
                CreateNoWindow = true,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                WorkingDirectory = _workingDirectory, 
                Arguments = FormQuery(imagesFolderPath, resultsFolderPath)
            };

            DirectoryInfo resultsDirectoryPathInfo = new(resultsFolderPath);
            if (!resultsDirectoryPathInfo.Exists)
                resultsDirectoryPathInfo.Create();

            string output = string.Empty;

            using (var process = Process.Start(processStartInfo))
            {
                output = process.StandardOutput.ReadToEnd();
            }

            output = output.Substring(output.IndexOf('\t') + 1).Trim('\r', '\n');

            return output;
        }

        private string FormQuery(string imagesFolderPath, string resultsFolderPath)
        {
            _queryBuilder.Clear();
            _queryBuilder.Append($"{_scriptName} ");
            _queryBuilder.Append($"-m {_modelFolderPath} ");
            _queryBuilder.Append($"-i {imagesFolderPath} ");
            _queryBuilder.Append($"-o {resultsFolderPath}");

            return _queryBuilder.ToString();
        }
    }
}
