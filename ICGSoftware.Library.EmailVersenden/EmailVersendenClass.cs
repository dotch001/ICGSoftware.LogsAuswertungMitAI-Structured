using Azure.Identity;
using Azure.Messaging;
using ICGSoftware.Library.CreateFirebirdDatabase;
using ICGSoftware.Library.Logging;
using ICGSoftware.Library.LogsAuswerten;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using Microsoft.Graph;
using Microsoft.Graph.Models;
using Microsoft.Graph.Users.Item.SendMail;
using Microsoft.Identity.Client;
using System.Net.Mail;
using System.Runtime;


namespace ICGSoftware.Library.EmailVersenden
{
    public class EmailVersendenClass(IOptions<AppSettingsClassDev> settings, IOptions<AppSettingsClassConf> confidential, LoggingClass loggingClass)
    {
        private readonly AppSettingsClassDev settings = settings.Value;
        private readonly AppSettingsClassConf authSettings = confidential.Value;
        private readonly LoggingClass loggingClass = loggingClass;




        public async Task Authentication(string Message)
        {
            try
            {

                var confidentialClient = ConfidentialClientApplicationBuilder
                    .Create(authSettings.ClientId)
                    .WithClientSecret(authSettings.ClientSecret)
                    .WithAuthority($"https://login.microsoftonline.com/{authSettings.TenantId}")
                    .Build();

                var scopes = new[] { "https://graph.microsoft.com/.default" };
                var authResult = await confidentialClient.AcquireTokenForClient(scopes).ExecuteAsync();
                var accessToken = authResult.AccessToken;

                await SendEmail(Message);


            }
            catch (Exception ex)
            {
                loggingClass.LoggerFunction("Error", $"Error sending email: {ex.Message}");
            }
        }

        public async Task SendEmail(string Message)
        {
            try 
            { 
            loggingClass.LoggerFunction("Info", "SendEmail started...");

            var scopes = new[] { "https://graph.microsoft.com/.default" };
            var options = new ClientSecretCredentialOptions
            {
                AuthorityHost = AzureAuthorityHosts.AzurePublicCloud
            };

            var clientSecretCredential = new ClientSecretCredential(
                authSettings.TenantId,
                authSettings.ClientId,
                authSettings.ClientSecret,
                options);

            var graphClient = new GraphServiceClient(clientSecretCredential, scopes);

            var emailMessage = new Message
            {
                Subject = authSettings.subject,
                Body = new ItemBody
                {
                    ContentType = BodyType.Html,
                    Content = Message
                },
                ToRecipients = authSettings.recipientEmails.Select(email => new Recipient
                {
                    EmailAddress = new EmailAddress { Address = email }
                }).ToList()
            };

            var sendMailBody = new SendMailPostRequestBody
            {
                Message = emailMessage,
                SaveToSentItems = true
            };

            await graphClient.Users[authSettings.senderEmail]
                .SendMail
                .PostAsync(sendMailBody);

            loggingClass.LoggerFunction("Info", "Email sent successfully."); }
            catch (Exception ex)
            {
                loggingClass.LoggerFunction("Error", $"SendEmail failed: {ex.Message}");
            }
        }
    }
}