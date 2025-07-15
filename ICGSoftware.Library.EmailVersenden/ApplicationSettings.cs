namespace ICGSoftware.Library.EmailVersenden
{

    public class ApplicationSettingsClass
    {
        public required string[] recipientEmails { get; set; }
        public required string senderEmail { get; set; }
        public required string subject { get; set; }
    }
    public class AuthenticationSettingsClass
    {
        public required string ClientId { get; set; }
        public required string ClientSecret { get; set; }
        public required string TenantId { get; set; }
    }
}
