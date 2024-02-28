namespace PathNav.ExperimentControl
{
    using CsvHelper.Configuration;
    
    public sealed class CreationDataFormatMap : ClassMap<CreationDataFormat>
    {
        public CreationDataFormatMap()
        {
            Map(m => m.ID).Index(0).Name("ID");
            Map(m => m.BLOCK_ID).Index(1).Name("BLOCK_ID");
            Map(m => m.MODEL).Index(2).Name("MODEL");
            Map(m => m.METHOD).Index(3).Name("METHOD");
            Map(m => m.ACTION).Index(4).Name("ACTION");
            Map(m => m.TIMESTAMP).Index(5).Name("TIMESTAMP");
        }
    }
}
