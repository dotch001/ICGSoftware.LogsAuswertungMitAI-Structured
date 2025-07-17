using ICGSoftware.Library.GetAppSettings;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Text.RegularExpressions;
using ICGSoftware.Library.CreateFirebirdDatabase;
using ICGSoftware.Library.Logging;
using System.Xml;
using ICGSoftware.Library;

namespace ICGSoftware.Library.ErrorsKategorisierenUndZaehlen
{
    public class ErrorsKategorisierenUndZaehlenClass(CreateFirebirdDatabaseClass createFirebirdDatabase)
    {
        CreateFirebirdDatabaseClass _createFirebirdDatabase = createFirebirdDatabase;
    

        Dictionary<string, List<string>> categoryTimestamps = new Dictionary<string, List<string>>();
        Dictionary<string, int> categoryCounts = new Dictionary<string, int>();

        public async Task ErrorsKategorisieren(string outputFolder)
        {
            string[] fileNames = Directory.GetFiles(outputFolder);

            foreach (var file in fileNames)
            {
                foreach (string line in File.ReadLines(file))
                {
                    await GetError(line);
                }
            }

            await WriteToFile(outputFolder);

            _createFirebirdDatabase.CreateDatabase(Path.Combine(outputFolder, "ErrorListe.json"));
        }

        public Task GetError(string line)
        {
            if (string.IsNullOrWhiteSpace(line) || !line.Contains("[ERR]"))
                return Task.CompletedTask;

            var timestampMatch = Regex.Match(line, @"^\d{4}-\d{2}-\d{2} \d{2}:\d{2}:\d{2}\.\d{3} [+\-]\d{2}:\d{2}");
            string timestamp = timestampMatch.Success ? timestampMatch.Value : "";

            string cleanedLine = Regex.Replace(line, "\".*?\"|'.*?'", "");

            int errIndex = cleanedLine.IndexOf("[ERR]");
            if (errIndex < 0)
                return Task.CompletedTask;

            string rawCategory = cleanedLine.Substring(errIndex);

            string noParams = Regex.Replace(rawCategory, @"\[Parameters=.*?\]", "[Parameters]");
            string normalized = Regex.Replace(noParams, @"\(\d+ms\)", "(ms)").Trim();

            int colonIndex = normalized.IndexOf(":");
            if (colonIndex > 0)
                normalized = normalized.Substring(0, colonIndex + 1);

            KategorisierenUndZählen(normalized, timestamp);


            
            return Task.CompletedTask;
        }


        public void KategorisierenUndZählen(string category, string timestamp)
        {
            if (string.IsNullOrWhiteSpace(category))
                category = "(no category)";

            if (!categoryCounts.ContainsKey(category))
            {
                categoryCounts[category] = 0;
                categoryTimestamps[category] = new List<string>();
            }

            categoryCounts[category]++;
            categoryTimestamps[category].Add(timestamp);
        }

        private async Task WriteToFile(string outputFolder)
        {
            string outputFile = Path.Combine(outputFolder, "ErrorListe.json");

            var allData = new JObject();

            foreach (var category in categoryCounts.Keys)
            {
                var inner = new JObject
                {
                    ["Aufgetreten"] = categoryCounts[category] + " mal",
                    ["Timestamps"] = JToken.FromObject(categoryTimestamps[category])
                };

                allData[category] = inner;
            }

            string jsonString = JsonConvert.SerializeObject(allData, Newtonsoft.Json.Formatting.Indented);

            await File.WriteAllTextAsync(outputFile, jsonString);
        }
    }
}
