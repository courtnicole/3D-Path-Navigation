namespace DataLogging
{
    using System.Globalization;

    public readonly struct Field
    {
        public Field(string logId, string logData)
        {
            _logId   = logId;
            _logData = logData;
        }

        private readonly string _logId;
        private readonly string _logData;

        public string LogId => _logId.ToString(CultureInfo.InvariantCulture);
        public string LogData => _logData.ToString(CultureInfo.InvariantCulture);
    }

}