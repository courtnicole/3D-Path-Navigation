namespace PathNav.ExperimentControl
{
    using CsvHelper;
    using CsvHelper.Configuration;
    using Dreamteck.Splines;
    using System.Collections.Generic;
    using System.Globalization;
    using System.IO;
    using System.Linq;
    using System.Threading.Tasks;

    public static class CsvLogger 
    {
        private static string _logFile;
        private static readonly CsvConfiguration Config = new (CultureInfo.InvariantCulture)
        {
            HasHeaderRecord = true,
            Delimiter       = ",",
        };
        private const string _endOfFile = "###################################################";
        private const string _endOfFileEarly = "***************************************************";
        private static readonly CsvHelper.TypeConversion.TypeConverterOptions FloatOptions = new()
        { 
            Formats = new[] { "###.000", }, 
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
            await using CsvWriter csvWriter = new(streamWriter, Config);
            csvWriter.Context.TypeConverterOptionsCache.AddOptions<float>(FloatOptions);
            csvWriter.Context.TypeConverterOptionsCache.AddOptions<double>(FloatOptions);
            csvWriter.Context.RegisterClassMap<SceneDataFormatMap>();
            csvWriter.WriteHeader<SceneDataFormat>();
            await csvWriter.NextRecordAsync();
        }

        public static async Task<bool> LogSceneData(SceneDataFormat data)
        {
            await using StreamWriter streamWriter = new (_logFile, true);
            await using CsvWriter    csvWriter    = new (streamWriter, Config);
            csvWriter.Context.RegisterClassMap<SceneDataFormatMap>();
            csvWriter.WriteRecord(data);
            await csvWriter.NextRecordAsync();
            return true;
        }
        
        public static async Task<bool> RecordEarlyTermination()
        {
            await using StreamWriter streamWriter = new (_logFile, true);
            await using CsvWriter    csvWriter    = new (streamWriter, Config);
            csvWriter.WriteComment(_endOfFileEarly);
            await csvWriter.NextRecordAsync();
            return true;
        }

        public static async Task<bool> LogSpline(string logDirectory, string filePath, IEnumerable<SplinePoint> splinePoints)
        {
            if (!Directory.Exists(logDirectory))
            {
                Directory.CreateDirectory(logDirectory);
            }
            
            await using StreamWriter streamWriter = new (filePath);
            await using CsvWriter    csvWriter    = new (streamWriter, Config);
            csvWriter.Context.RegisterClassMap<SplinePointMap>();
            csvWriter.WriteField("PositionX");
            csvWriter.WriteField("PositionY");
            csvWriter.WriteField("PositionZ");
            csvWriter.WriteField("TangentX");
            csvWriter.WriteField("TangentY");
            csvWriter.WriteField("TangentZ");
            csvWriter.WriteField("Tangent2X");
            csvWriter.WriteField("Tangent2Y");
            csvWriter.WriteField("Tangent2Z");
            csvWriter.WriteField("NormalX");
            csvWriter.WriteField("NormalY");
            csvWriter.WriteField("NormalZ");
            csvWriter.WriteField("Size");
            await csvWriter.NextRecordAsync();

            foreach (SplinePoint point in splinePoints)
            {
                csvWriter.WriteRecord(point.position.x);
                csvWriter.WriteRecord(point.position.y);
                csvWriter.WriteRecord(point.position.z);
                csvWriter.WriteRecord(point.tangent.x);
                csvWriter.WriteRecord(point.tangent.y);
                csvWriter.WriteRecord(point.tangent.z);
                csvWriter.WriteRecord(point.tangent2.x);
                csvWriter.WriteRecord(point.tangent2.y);
                csvWriter.WriteRecord(point.tangent2.z);
                csvWriter.WriteRecord(point.normal.x);
                csvWriter.WriteRecord(point.normal.y);
                csvWriter.WriteRecord(point.normal.z);
                csvWriter.WriteRecord(point.size);
                await csvWriter.NextRecordAsync();
            }
            return true;
        }

        public static SplinePoint[] ReadSpline(string filePath)
        { 
            using StreamReader    streamReader = new (filePath);
            using CsvReader csvReader    = new (streamReader, CultureInfo.InvariantCulture);
            csvReader.Context.RegisterClassMap<SplinePointMap>();
            return csvReader.GetRecords<SplinePoint>().ToArray();
        }

        public static async Task<bool> FinalizeDataLog()
        {
            await using StreamWriter streamWriter = new (_logFile, true);
            await using CsvWriter    csvWriter    = new (streamWriter, Config);
            csvWriter.WriteComment(_endOfFile);
            await csvWriter.NextRecordAsync();
            return true;
        }
    }
}
