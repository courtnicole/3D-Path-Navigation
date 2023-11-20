namespace PathNav.ExperimentControl
{
    using CsvHelper;
    using CsvHelper.Configuration;
    using System.Globalization;
    using System.IO;
    using System.Threading.Tasks;

    public static class CsvLogger 
    {
        private static string _logFile;
        private static readonly CsvConfiguration Config = new (CultureInfo.InvariantCulture)
        {
            HasHeaderRecord = true,
        };

        public static async Task InitSceneDataLog(string logDirectory, string filePath)
        {
            if (!Directory.Exists(logDirectory))
            {
                Directory.CreateDirectory(logDirectory);
            }
            
            _logFile = filePath;
            if (File.Exists(_logFile)) return;
            await using StreamWriter streamWriter = new (_logFile);
            await using CsvWriter    csvWriter    = new (streamWriter, Config);
            csvWriter.Context.RegisterClassMap<SceneDataFormatMap>();
            csvWriter.WriteHeader<SceneDataFormat>();
            await csvWriter.NextRecordAsync();
        }

        public static async Task LogSceneData(SceneDataFormat data)
        {
            await using StreamWriter streamWriter = new (_logFile);
            await using CsvWriter    csvWriter    = new (streamWriter, Config);
            csvWriter.Context.RegisterClassMap<SceneDataFormatMap>();
            csvWriter.WriteRecord(data);
            await csvWriter.NextRecordAsync();
        }
    }
}
