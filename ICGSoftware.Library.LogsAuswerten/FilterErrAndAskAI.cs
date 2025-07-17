using ICGSoftware.GetAppSettings;
using Microsoft.Extensions.Options;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.RegularExpressions;


namespace ICGSoftware.LogsAuswerten
{
    public class FilterErrAndAskAIClass(IOptions<AppSettingsClassDev> settings, IOptions<AppSettingsClassConf> confidential, Logging.Logging loggingClass)
    {
        private readonly AppSettingsClassDev settings = settings.Value;
        private readonly AppSettingsClassConf confidential = confidential.Value;
        private readonly Logging.Logging _LoggingClass = loggingClass;


        private string outputFile = "";
        private string outputFolder = "";

        public async Task<string> FilterErrAndAskAI(ErrorsKategorisierenUndZaehlen.ErrorsKategorisierenUndZaehlen errorsKategorisierenUndZaehlenClass, CancellationToken stoppingToken)
        {

            // Declaring variables
            int amountOfFiles;
            string[] fileNames;

            string outputFileOld = outputFile;
            string outputFilePath = "";

            string inputPath;

            string endTermOld;
            string endTermNew;

            bool isBetween = false;

            bool found = false;

            int madeNewFilesCount = 0;

            var overwritePrevention = 0;

            string fileAsText = "";

            string allResponses = "";
            string completeOutputFolder = "";

            try
            {
                // Looping through all input folder paths
                for (int i = 0; i < settings.inputFolderPaths.Length; i++)
                {
                    stoppingToken.ThrowIfCancellationRequested();
                    // or check periodically
                    if (stoppingToken.IsCancellationRequested) return "";

                    // Getting input folder paths and files (for reading and naming)
                    amountOfFiles = Directory.GetFiles(settings.inputFolderPaths[i]).Length;
                    fileNames = Directory.GetFiles(settings.inputFolderPaths[i]);



                    outputFolder = settings.outputFolderPath + "\\ExtentionLogsFolder" + overwritePrevention;
                    if (!Directory.Exists(outputFolder)) { Directory.CreateDirectory(outputFolder); }
                    else
                    {
                        while (Directory.Exists(outputFolder))
                        {
                            overwritePrevention++;
                            outputFolder = settings.outputFolderPath + "\\ExtentionLogsFolder" + overwritePrevention;
                        }
                        Directory.CreateDirectory(outputFolder);

                    }

                    completeOutputFolder = settings.outputFolderPath + "\\ExtentionLogsFolder" + overwritePrevention;

                    DateTime now = DateTime.Now;
                    string TodaysDate = DateTime.Today.ToString("yyyyMMdd");

                    _LoggingClass.LoggerFunction("Info", "Date: " + TodaysDate);

                    // Defining endTermOld (when endTermOld != endTermNew make new folder for different days)
                    endTermOld = fileNames[0].Replace(settings.inputFolderPaths[i] + "\\TritomWeb.Api", "").Substring(0, 4);

                    // Declaring a list to store extracted lines
                    List<string> extractedLines = new List<string>();

                    // Looping through all files in the input current folder
                    for (int j = 0; j < amountOfFiles; j++)
                    {
                        stoppingToken.ThrowIfCancellationRequested();
                        // or check periodically
                        if (stoppingToken.IsCancellationRequested) return "";

                        if (fileNames[j].Contains(TodaysDate)) { continue; }

                        // OutputFilePath is declared
                        outputFilePath = Path.Combine(outputFolder + "\\ExtentionLog" + fileNames[j].Replace(settings.inputFolderPaths[i] + "\\TritomWeb.Api", "").Substring(0, 8));

                        // OutputFile is changed to include the file name and the number of files made
                        outputFile = Path.Combine(outputFilePath + "_" + madeNewFilesCount + ".txt");

                        // OutputFileOld is used to check if a new file is needed
                        if (outputFile.Split("_")[0] + ".txt" != outputFileOld.Split("_")[0] + ".txt")
                        {
                            extractedLines.Clear();
                            madeNewFilesCount = 0;
                            outputFileOld = outputFile;
                        }

                        // Declaring endTermNew (for comparing with endTermOld)
                        endTermNew = fileNames[j].Replace(settings.inputFolderPaths[i] + "\\TritomWeb.Api", "").Substring(0, 4);

                        // Checking if new output file is needed
                        if (endTermOld != endTermNew)
                        {
                            outputFile = Path.Combine(outputFilePath + "_" + madeNewFilesCount + ".txt");
                            outputFileOld = outputFile;
                            endTermOld = endTermNew;
                        }

                        // InputPath of current file
                        inputPath = fileNames[j];


                        // Resetting the found bool (whether the startTerm has been found) for the current loop
                        found = false;

                        // Checking if the startTerm is in the file
                        using (StreamReader reader = new StreamReader(inputPath))
                        {
                            string? lineread;
                            while ((lineread = reader.ReadLine()) != null)
                            {
                                if (lineread.Contains(settings.startTerm, StringComparison.OrdinalIgnoreCase))
                                {
                                    found = true;
                                }
                            }
                        }

                        // If the startTerm is not found, continue to the next file
                        if (!found)
                        {
                            // Informs of no errors
                            ConsoleLogsAndInformation(settings.inform, ((j + 1) + " fertig von " + amountOfFiles + " (Kein Error gefunden)"));
                            continue;
                        }
                        // If the startTerm is found, continue with extracting lines
                        else
                        {
                            // Scanning each line in the file
                            foreach (string line in File.ReadLines(inputPath))
                            {
                                // Checking if the line contains the endTerm (if so extracts all lines and resets isBetween)
                                if (line.Contains(endTermOld) && isBetween)
                                {
                                    isBetween = false;
                                    File.WriteAllLines(outputFile, extractedLines);

                                    if (File.Exists(outputFile))
                                    {
                                        FileInfo fileInfo = new FileInfo(outputFile);
                                        long fileSize = fileInfo.Length;
                                        ConsoleLogsAndInformation(settings.inform, $"File size: {fileSize / 1024} KB of file {fileInfo.Name}");

                                        if (fileSize / 1024 >= settings.maxSizeInKB - 20)
                                        {
                                            madeNewFilesCount++;
                                            outputFile = Path.Combine(outputFilePath + "_" + madeNewFilesCount + ".txt");
                                            outputFileOld = outputFile;
                                            ConsoleLogsAndInformation(settings.inform, "Neue Datei erstellt: " + outputFile);
                                            extractedLines.Clear();
                                        }
                                    }
                                }

                                // Checking if the line contains the startTerm (if so sets isBetween to true)
                                if (line.Contains(settings.startTerm)) { isBetween = true; }

                                // Checking if the line contains the endTerm (if so adds the line to extracted lines list)
                                if (isBetween) { extractedLines.Add(line); }
                            }
                        }

                        // Informs about the progress
                        ConsoleLogsAndInformation(settings.inform, (j + 1) + " fertig von " + amountOfFiles);
                    }
                    await errorsKategorisierenUndZaehlenClass.ErrorsKategorisieren(completeOutputFolder);


                    // Asks AI about the files in the output folder (for each file)
                    if (settings.AskAI)
                    {
                        _LoggingClass.LoggerFunction("Info", "Asking AI");
                        for (int k = 0; k < Directory.GetFiles(outputFolder).Length; k++)
                        {
                            string[] filesInOutput = Directory.GetFiles(outputFolder);
                            string PathToFile = filesInOutput[k];
                            string response = await AskAndGetResponse(outputFolder, k, fileAsText, stoppingToken);
                            allResponses = allResponses + $"<b><br /><br />----------------------------------------------{PathToFile}----------------------------------------------<br /><br /></b>" + response;
                            ConsoleLogsAndInformation(settings.inform, response);
                        }
                    }


                }

                return allResponses;
            }
            catch (Exception ex)
            {
                _LoggingClass.LoggerFunction("Error", $"from FilterErrAndAskAI: {ex.Message}");
                return "Error from FilterErrAndAskAI: " + ex.Message;
            }
        }

        public async Task<string> AskAndGetResponse(string outputFolder, int k, string fileAsText, CancellationToken stoppingToken)
        {
            string[] filesInOutput = Directory.GetFiles(outputFolder);
            string PathToFile = filesInOutput[k];

            using (StreamReader reader = new StreamReader(PathToFile))
            {
                fileAsText = reader.ReadToEnd();
            }

            await Task.Delay(1000);
            ConsoleLogsAndInformation(settings.inform, $"\n\n----------------------------------------------{PathToFile}----------------------------------------------\n\n");
            string model = settings.models[settings.chosenModel];
            string response = await AskQuestionAboutFile(fileAsText, model, stoppingToken);
            return response;
        }


        public void ConsoleLogsAndInformation(bool inform, string theInformation)
        {
            if (inform)
            {
                Console.WriteLine(theInformation);
            }
        }

        //ask AI about a file
        public async Task<string> AskQuestionAboutFile(string FileAsText, string model, CancellationToken stoppingToken)
        {
            stoppingToken.ThrowIfCancellationRequested();
            // or check periodically
            if (stoppingToken.IsCancellationRequested) return "";
            var apiUrl = "https://openrouter.ai/api/v1/chat/completions";

            var client = new HttpClient();
            client.DefaultRequestHeaders.Add("Authorization", $"Bearer {confidential.ApiKey}");

            var requestBody = new
            {
                model = model,
                messages = new[]
                {
            new { role = "user", content = settings.Question + FileAsText }
            }
            };

            var content = new StringContent(JsonSerializer.Serialize(requestBody), Encoding.UTF8, "application/json");

            var response = await client.PostAsync(apiUrl, content);

            if (!response.IsSuccessStatusCode)
            {
                ConsoleLogsAndInformation(settings.inform, $"Error: {response.StatusCode}");
                ConsoleLogsAndInformation(settings.inform, await response.Content.ReadAsStringAsync());
                return "[API error]";
            }

            var responseString = await response.Content.ReadAsStringAsync();

            var json = JsonNode.Parse(responseString);
            var messageContent = json?["choices"]?[0]?["message"]?["content"]?.ToString();

            var cleanedContent = Regex.Replace(messageContent ?? "", @"\n{3,}", "\n\n").Trim();

            return string.IsNullOrWhiteSpace(cleanedContent) ? "Raw Response: " + responseString : cleanedContent;


        }
    }
}
