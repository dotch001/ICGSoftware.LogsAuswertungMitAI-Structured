using ICGSoftware.GetAppSettings;
using Microsoft.Extensions.Options;
using Serilog;
using Serilog.Core;

namespace ICGSoftware.Logging
{
    public class Logging(IOptions<AppSettingsClassDev> settings)
    {
        private readonly AppSettingsClassDev settings = settings.Value;

        public void LoggerFunction(string TypeOfMessage, string message)
        {

            if (!Directory.Exists(settings.outputFolderPath + "\\Logs")) { Directory.CreateDirectory(settings.outputFolderPath + "\\Logs"); }

            int i = 0;

            string outputFile = settings.outputFolderPath + "\\Logs\\" + settings.logFileName + i + ".log";

            bool isLoggerConfigured = Log.Logger != Logger.None;

            while (File.Exists(outputFile) && new FileInfo(outputFile).Length / 1024 >= 300)
            {
                i++;
                outputFile = settings.outputFolderPath + "\\Logs\\" + settings.logFileName + i + ".log";
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
