namespace PathNav.ExperimentControl
{
    using CsvHelper;
    using CsvHelper.Configuration;
    using System.Globalization;
    using System.IO;
    using System.Threading.Tasks;

    public static class CsvLogger 
    {
        private static string _logDirectory;
        private static CsvWriter _csvWriter;
        private static StreamWriter _streamWriter;
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
            
            _logDirectory = filePath;
            bool             fileExists = File.Exists(_logDirectory);

            _streamWriter = new StreamWriter(_logDirectory);
            _csvWriter    = new CsvWriter(_streamWriter, Config);
            _csvWriter.Context.RegisterClassMap<SceneDataFormatMap>();

            if (fileExists) return;
            
            _csvWriter.WriteHeader<SceneDataFormat>();
            await _csvWriter.NextRecordAsync();
        }

        public static async Task LogSceneData(SceneDataFormat data)
        {
            _csvWriter.WriteRecord(data);
            await _csvWriter.NextRecordAsync();
        }

        public static async Task EndLogging()
        {
             await _csvWriter.DisposeAsync();
             await _streamWriter.DisposeAsync();
        }

    }
}
