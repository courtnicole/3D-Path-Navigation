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
            Map(m => m.HEAD_POSITION).Index(7).Name("HEAD_POSITION");
            Map(m => m.HEAD_ROTATION).Index(8).Name("HEAD_ROTATION");
            Map(m => m.LEFT_POSITION).Index(9).Name("LEFT_POSITION");
            Map(m => m.LEFT_ROTATION).Index(10).Name("LEFT_ROTATION");
            Map(m => m.RIGHT_POSITION).Index(11).Name("RIGHT_POSITION");
            Map(m => m.RIGHT_ROTATION).Index(12).Name("RIGHT_ROTATION");
            Map(m => m.TRACKED_HEAD_POSITION).Index(13).Name("TRACKED_HEAD_POSITION");
            Map(m => m.TRACKED_HEAD_ROTATION).Index(14).Name("TRACKED_HEAD_ROTATION");
            Map(m => m.TRACKED_LEFT_POSITION).Index(15).Name("TRACKED_LEFT_POSITION");
            Map(m => m.TRACKED_LEFT_ROTATION).Index(16).Name("TRACKED_LEFT_ROTATION");
            Map(m => m.TRACKED_RIGHT_POSITION).Index(17).Name("TRACKED_RIGHT_POSITION");
            Map(m => m.TRACKED_RIGHT_ROTATION).Index(18).Name("TRACKED_RIGHT_ROTATION");
            Map(m => m.TIMESTAMP).Index(19).Name("TIMESTAMP");
        }
    }
}
