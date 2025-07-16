namespace ICGSoftware.Library.GetAppSettings
{ 
    public class AppSettingsClassDev
    {
        public required string outputFolderForDB { get; set; }
        public required string DBUser { get; set; }
        public required string Question { get; set; }
        public required string startTerm { get; set; }
        public required string[] inputFolderPaths { get; set; }
        public string outputFolderPath { get; set; }
        public bool inform { get; set; }
        public bool AskAI { get; set; }
        public required string[] models { get; set; }
        public int chosenModel { get; set; }
        public int maxSizeInKB { get; set; }
        public required string outputFolderForLogs { get; set; }
        public required string logFileName { get; set; }
        
    }
    public class AppSettingsClassConf
    {
        public required string subject { get; set; }
        public required string senderEmail { get; set; }
        public required string[] recipientEmails { get; set; }
        public required string DBPassword { get; set; }
        public required string ApiKey { get; set; }
        public required string ClientId { get; set; }
        public required string ClientSecret { get; set; }
        public required string TenantId { get; set; }
    }
}
