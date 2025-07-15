using Microsoft.Extensions.Configuration;
using Serilog;
using Serilog.Core;

namespace ICGSoftware.Library.Logging
{
    public class LoggingClass
    {
        public static void LoggerFunction(string TypeOfMessage, string message)
        {
            var config = new ConfigurationBuilder()
                .AddJsonFile(Directory.GetParent(Directory.GetCurrentDirectory()).FullName + "\\ICGSoftware.LogsAuswertungMitAI\\appsettings.Development.json")
                .Build();

            var settings = config.GetSection("AppSettings").Get<ApplicationSettingsClass>();

            if (!Directory.Exists(settings.outputFolderForLogs)) { Directory.CreateDirectory(settings.outputFolderForLogs); }

            int i = 0;

            string outputFile = settings.outputFolderForLogs + "\\" + settings.logFileName + i + ".log";

            bool isLoggerConfigured = Log.Logger != Logger.None;

            while (File.Exists(outputFile) && new FileInfo(outputFile).Length / 1024 >= 300)
            {
                i++;
                outputFile = settings.outputFolderForLogs + "\\" + settings.logFileName + i + ".log";
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
