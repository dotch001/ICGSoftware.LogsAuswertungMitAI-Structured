using Azure.Identity;
using ICGSoftware.GetAppSettings;
using Microsoft.Extensions.Options;
using Microsoft.Graph;
using Microsoft.Graph.Models;
using Microsoft.Graph.Users.Item.SendMail;
using Microsoft.Identity.Client;


namespace ICGSoftware.EmailVersenden
{
    public class EmailVersenden(IOptions<AppSettingsClassConf> confidential, Logging.Logging loggingClass)
    {
        private readonly AppSettingsClassConf confidential = confidential.Value;
        private readonly Logging.Logging _LoggingClass = loggingClass;




        public async Task Authentication(string Message)
        {
            try
            {

                var confidentialClient = ConfidentialClientApplicationBuilder
                    .Create(confidential.ClientId)
                    .WithClientSecret(confidential.ClientSecret)
                    .WithAuthority($"https://login.microsoftonline.com/{confidential.TenantId}")
                    .Build();

                var scopes = new[] { "https://graph.microsoft.com/.default" };
                var authResult = await confidentialClient.AcquireTokenForClient(scopes).ExecuteAsync();
                var accessToken = authResult.AccessToken;

                await SendEmail(Message);


            }
            catch (Exception ex)
            {
                _LoggingClass.LoggerFunction("Error from Authentication:", $"Error sending email: {ex.Message}");
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
                    confidential.TenantId,
                    confidential.ClientId,
                    confidential.ClientSecret,
                    options);

                var graphClient = new GraphServiceClient(clientSecretCredential, scopes);

                var emailMessage = new Message
                {
                    Subject = confidential.subject,
                    Body = new ItemBody
                    {
                        ContentType = BodyType.Html,
                        Content = Message
                    },
                    ToRecipients = confidential.recipientEmails.Select(email => new Recipient
                    {
                        EmailAddress = new EmailAddress { Address = email }
                    }).ToList()
                };

                var sendMailBody = new SendMailPostRequestBody
                {
                    Message = emailMessage,
                    SaveToSentItems = true
                };

                await graphClient.Users[confidential.senderEmail]
                    .SendMail
                    .PostAsync(sendMailBody);

                _LoggingClass.LoggerFunction("Info", "Email sent successfully.");
            }
            catch (Exception ex)
            {
                _LoggingClass.LoggerFunction("Error from SendEmail", $"SendEmail failed: {ex.Message}");
            }
        }
    }
}