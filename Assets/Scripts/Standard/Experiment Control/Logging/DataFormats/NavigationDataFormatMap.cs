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
            Map(m => m.HEAD_POSITION_X).Index(7).Name("HEAD_POSITION_X");
            Map(m => m.HEAD_POSITION_Y).Index(8).Name("HEAD_POSITION_Y");
            Map(m => m.HEAD_POSITION_Z).Index(9).Name("HEAD_POSITION_Z");
            Map(m => m.HEAD_ROTATION_X).Index(10).Name("HEAD_ROTATION_X");
            Map(m => m.HEAD_ROTATION_Y).Index(11).Name("HEAD_ROTATION_Y");
            Map(m => m.HEAD_ROTATION_Z).Index(12).Name("HEAD_ROTATION_Z");
            Map(m => m.HEAD_ROTATION_W).Index(13).Name("HEAD_ROTATION_W");
            Map(m => m.LEFT_POSITION_X).Index(14).Name("LEFT_POSITION_X");
            Map(m => m.LEFT_POSITION_Y).Index(15).Name("LEFT_POSITION_Y");
            Map(m => m.LEFT_POSITION_Z).Index(16).Name("LEFT_POSITION_Z");
            Map(m => m.LEFT_ROTATION_X).Index(17).Name("LEFT_ROTATION_X");
            Map(m => m.LEFT_ROTATION_Y).Index(18).Name("LEFT_ROTATION_Y");
            Map(m => m.LEFT_ROTATION_Z).Index(19).Name("LEFT_ROTATION_Z");
            Map(m => m.LEFT_ROTATION_W).Index(20).Name("LEFT_ROTATION_W");
            Map(m => m.RIGHT_POSITION_X).Index(21).Name("RIGHT_POSITION_X");
            Map(m => m.RIGHT_POSITION_Y).Index(22).Name("RIGHT_POSITION_Y");
            Map(m => m.RIGHT_POSITION_Z).Index(23).Name("RIGHT_POSITION_Z");
            Map(m => m.RIGHT_ROTATION_X).Index(24).Name("RIGHT_ROTATION_X");
            Map(m => m.RIGHT_ROTATION_Y).Index(25).Name("RIGHT_ROTATION_Y");
            Map(m => m.RIGHT_ROTATION_Z).Index(26).Name("RIGHT_ROTATION_Z");
            Map(m => m.RIGHT_ROTATION_W).Index(27).Name("RIGHT_ROTATION_W");
            Map(m => m.TRACKED_HEAD_POSITION_X).Index(28).Name("TRACKED_HEAD_POSITION_X");
            Map(m => m.TRACKED_HEAD_POSITION_Y).Index(29).Name("TRACKED_HEAD_POSITION_Y");
            Map(m => m.TRACKED_HEAD_POSITION_Z).Index(30).Name("TRACKED_HEAD_POSITION_Z");
            Map(m => m.TRACKED_HEAD_ROTATION_X).Index(31).Name("TRACKED_HEAD_ROTATION_X");
            Map(m => m.TRACKED_HEAD_ROTATION_Y).Index(32).Name("TRACKED_HEAD_ROTATION_Y");
            Map(m => m.TRACKED_HEAD_ROTATION_Z).Index(33).Name("TRACKED_HEAD_ROTATION_Z");
            Map(m => m.TRACKED_HEAD_ROTATION_W).Index(34).Name("TRACKED_HEAD_ROTATION_W");
            Map(m => m.TRACKED_LEFT_POSITION_X).Index(35).Name("TRACKED_LEFT_POSITION_X");
            Map(m => m.TRACKED_LEFT_POSITION_Y).Index(36).Name("TRACKED_LEFT_POSITION_Y");
            Map(m => m.TRACKED_LEFT_POSITION_Z).Index(37).Name("TRACKED_LEFT_POSITION_Z");
            Map(m => m.TRACKED_LEFT_ROTATION_X).Index(38).Name("TRACKED_LEFT_ROTATION_X");
            Map(m => m.TRACKED_LEFT_ROTATION_Y).Index(39).Name("TRACKED_LEFT_ROTATION_Y");
            Map(m => m.TRACKED_LEFT_ROTATION_Z).Index(40).Name("TRACKED_LEFT_ROTATION_Z");
            Map(m => m.TRACKED_LEFT_ROTATION_W).Index(41).Name("TRACKED_LEFT_ROTATION_W");
            Map(m => m.TRACKED_RIGHT_POSITION_X).Index(42).Name("TRACKED_RIGHT_POSITION_X");
            Map(m => m.TRACKED_RIGHT_POSITION_Y).Index(43).Name("TRACKED_RIGHT_POSITION_Y");
            Map(m => m.TRACKED_RIGHT_POSITION_Z).Index(44).Name("TRACKED_RIGHT_POSITION_Z");
            Map(m => m.TRACKED_RIGHT_ROTATION_X).Index(45).Name("TRACKED_RIGHT_ROTATION_X");
            Map(m => m.TRACKED_RIGHT_ROTATION_Y).Index(46).Name("TRACKED_RIGHT_ROTATION_Y");
            Map(m => m.TRACKED_RIGHT_ROTATION_Z).Index(47).Name("TRACKED_RIGHT_ROTATION_Z");
            Map(m => m.TRACKED_RIGHT_ROTATION_W).Index(48).Name("TRACKED_RIGHT_ROTATION_W");
            Map(m => m.TIMESTAMP).Index(49).Name("TIMESTAMP");
        }
    }
}
