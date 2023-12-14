namespace PathNav.ExperimentControl
{
    using CsvHelper.Configuration;
    public sealed class NavigationDataFormatMap  : ClassMap<NavigationDataFormat>
    {
        public NavigationDataFormatMap()
        {
            Map(m => m.ID).Index(0).Name("ID");
            Map(m => m.BLOCK_ID).Index(1).Name("BLOCK_ID");
            Map(m => m.MODEL).Index(2).Name("MODEL");
            Map(m => m.METHOD).Index(3).Name("METHOD");
            Map(m => m.SPEED).Index(4).Name("SPEED");
            Map(m => m.SPLINE_PERCENT).Index(5).Name("SPLINE_PERCENT");
            Map(m => m.SPLINE_POSITION).Index(6).Name("SPLINE_POSITION");
            Map(m => m.POSITION).Index(7).Name("POSITION");
            Map(m => m.ROTATION).Index(8).Name("ROTATION");
            Map(m => m.TIMESTAMP).Index(9).Name("TIMESTAMP");
        }
    }
}
