using ICGSoftware.Service;
using ICGSoftware.Library.CreateFirebirdDatabase;
using ICGSoftware.Library.EmailVersenden;
using ICGSoftware.Library.ErrorsKategorisierenUndZaehlen;
using ICGSoftware.Library.Logging;
using ICGSoftware.Library.LogsAuswerten;
using Microsoft.Extensions.Options;
using ICGSoftware.Library.GetAppSettings;

namespace ICGSoftware.Service
{
    public class Worker : BackgroundService
    {
        private readonly AppSettingsClassDev _appSettingsClassDev;
        private readonly AppSettingsClassConf _appSettingsClassConf;
        private readonly FilterErrAndAskAIClass _FilterErrAndAskAIClass;
        private readonly LoggingClass _LoggingClass;
        private readonly ErrorsKategorisierenUndZaehlenClass _ErrorsKategorisierenUndZaehlenClass;
        private readonly EmailVersendenClass _EmailVersendenClass;
        private readonly CreateFirebirdDatabaseClass _CreateFirebirdDatabaseClass;


        public Worker(IOptions<AppSettingsClassDev> appSettingsClassDev, IOptions<AppSettingsClassConf> appSettingsClassConf, FilterErrAndAskAIClass FilterErrAndAskAIClassSettings, LoggingClass loggingClass, EmailVersendenClass EmailVersendenClassSettings, ErrorsKategorisierenUndZaehlenClass ErrorsKategorisierenUndZaehlenClassSettings, CreateFirebirdDatabaseClass CreateFirebirdDatabaseClassSettings)
        {
            _LoggingClass = loggingClass;
            _appSettingsClassDev = appSettingsClassDev.Value;
            _appSettingsClassConf = appSettingsClassConf.Value;
            _FilterErrAndAskAIClass = FilterErrAndAskAIClassSettings;
            _EmailVersendenClass = EmailVersendenClassSettings;
            _ErrorsKategorisierenUndZaehlenClass = ErrorsKategorisierenUndZaehlenClassSettings;
            _CreateFirebirdDatabaseClass = CreateFirebirdDatabaseClassSettings;

        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _LoggingClass.LoggerFunction("Info", "Worker started");
            while(!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    while (!stoppingToken.IsCancellationRequested)
                    {
                        _LoggingClass.LoggerFunction("Info", "Worker started");
                        await Task.Delay(1000, stoppingToken);
                        string aiResponse = await _FilterErrAndAskAIClass.FilterErrAndAskAI(_ErrorsKategorisierenUndZaehlenClass, stoppingToken);
                        await _EmailVersendenClass.Authentication(aiResponse);
                        await Task.Delay(_appSettingsClassDev.IntervallInSeconds * 1000, stoppingToken);
                        _LoggingClass.LoggerFunction("Info", "Worker finished");
                    }

                }
                catch (Exception ex)
                {
                    _LoggingClass.LoggerFunction("Error", ex + " Worker crashed");
                    break;
                }
            }
        }
    }
}