namespace PathNav.ExperimentControl
{
    using CsvHelper.Configuration;
    public sealed class NavigationDataFormatMap  : ClassMap<NavigationDataFormat>
    {
        public NavigationDataFormatMap()
        {
            Map(m => m.ID).Index(0).Name("ID");
            Map(m => m.ID).Index(1).Name("BLOCK_ID");
            Map(m => m.ID).Index(2).Name("MODEL");
            Map(m => m.ID).Index(3).Name("METHOD");
            Map(m => m.ID).Index(4).Name("SPEED");
            Map(m => m.ID).Index(5).Name("POSITION");
            Map(m => m.ID).Index(6).Name("ROTATION");
            Map(m => m.ID).Index(7).Name("TIMESTAMP");
        }
    }
}
