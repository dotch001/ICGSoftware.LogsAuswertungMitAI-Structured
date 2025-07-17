using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using Serilog;
using Serilog.Core;
using ICGSoftware.Library.GetAppSettings;
using System.Runtime;

namespace ICGSoftware.Library.Logging
{
    public class LoggingClass(IOptions<AppSettingsClassDev> settings)
    {
        private readonly AppSettingsClassDev _settings = settings.Value;
    
        public void LoggerFunction(string TypeOfMessage, string message)
        {

            if (!Directory.Exists(_settings.outputFolderPath + "\\Logs")) { Directory.CreateDirectory(_settings.outputFolderPath + "\\Logs"); }

            int i = 0;

            string outputFile = _settings.outputFolderPath + "\\Logs\\" + _settings.logFileName + i + ".log";

            bool isLoggerConfigured = Log.Logger != Logger.None;

            while (File.Exists(outputFile) && new FileInfo(outputFile).Length / 1024 >= 300)
            {
                i++;
                outputFile = _settings.outputFolderPath + "\\Logs\\" + _settings.logFileName + i + ".log";
            }
            if (!isLoggerConfigured)
            {
                Log.Logger = new LoggerConfiguration().WriteTo.File(outputFile).CreateLogger();
            }


            if (TypeOfMessage == "Info")
            {
                Log.Information(message);
            }
            else if (TypeOfMessage == "Warning")
            {
                Log.Warning(message);
            }
            else if (TypeOfMessage == "Error")
            {
                Log.Error(message);
            }
            else if (TypeOfMessage == "Debug")
            {
                Log.Debug(message);
            }
        }
    }
}
