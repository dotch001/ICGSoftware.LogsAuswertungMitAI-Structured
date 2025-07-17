using ICGSoftware.GetAppSettings;
using ICGSoftware.LogsAuswerten;
using Microsoft.Extensions.Options;

namespace ICGSoftware.Service
{
    public class Worker : BackgroundService
    {
        private readonly AppSettingsClassDev _appSettingsClassDev;
        private readonly AppSettingsClassConf _appSettingsClassConf;
        private readonly FilterErrAndAskAIClass _FilterErrAndAskAI;
        private readonly Logging.Logging _Logging;
        private readonly ErrorsKategorisierenUndZaehlen.ErrorsKategorisierenUndZaehlen _ErrorsKategorisierenUndZaehlen;
        private readonly EmailVersenden.EmailVersenden _EmailVersenden;
        private readonly CreateFirebirdDatabase.CreateFirebirdDatabase _CreateFirebirdDatabase;


        public Worker(IOptions<AppSettingsClassDev> appSettingsClassDev, IOptions<AppSettingsClassConf> appSettingsClassConf, FilterErrAndAskAIClass FilterErrAndAskAIClassSettings, Logging.Logging loggingClass, EmailVersenden.EmailVersenden EmailVersendenClassSettings, ErrorsKategorisierenUndZaehlen.ErrorsKategorisierenUndZaehlen ErrorsKategorisierenUndZaehlenClassSettings, CreateFirebirdDatabase.CreateFirebirdDatabase CreateFirebirdDatabaseClassSettings)
        {
            _Logging = loggingClass;
            _appSettingsClassDev = appSettingsClassDev.Value;
            _appSettingsClassConf = appSettingsClassConf.Value;
            _FilterErrAndAskAI = FilterErrAndAskAIClassSettings;
            _EmailVersenden = EmailVersendenClassSettings;
            _ErrorsKategorisierenUndZaehlen = ErrorsKategorisierenUndZaehlenClassSettings;
            _CreateFirebirdDatabase = CreateFirebirdDatabaseClassSettings;

        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _Logging.LoggerFunction("Info", "Worker started");
            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    while (!stoppingToken.IsCancellationRequested)
                    {
                        _Logging.LoggerFunction("Info", "Worker started");
                        await Task.Delay(1000, stoppingToken);
                        string aiResponse = await _FilterErrAndAskAI.FilterErrAndAskAI(_ErrorsKategorisierenUndZaehlen, stoppingToken);
                        await _EmailVersenden.Authentication(aiResponse);
                        await Task.Delay(_appSettingsClassDev.IntervallInSeconds * 1000, stoppingToken);
                        _Logging.LoggerFunction("Info", "Worker finished");
                    }

                }
                catch (Exception ex)
                {
                    _Logging.LoggerFunction("Error", ex + " Worker crashed");
                    break;
                }
            }
        }
    }
}