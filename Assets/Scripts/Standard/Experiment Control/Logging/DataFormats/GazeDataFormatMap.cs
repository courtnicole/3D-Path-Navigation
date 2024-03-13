
namespace PathNav.ExperimentControl
{
    using CsvHelper.Configuration;

    public sealed class GazeDataFormatMap : ClassMap<GazeDataFormat>
    {
        public GazeDataFormatMap()
        {
            Map(m => m.ID).Index(0).Name("ID");
            Map(m => m.BLOCK_ID).Index(1).Name("BLOCK_ID");
            Map(m => m.MODEL).Index(2).Name("MODEL");
            Map(m => m.METHOD).Index(3).Name("METHOD");
            Map(m => m.CONVERGENCE_DISTANCE).Index(4).Name("CONVERGENCE_DISTANCE");
            Map(m => m.CONVERGENCE_VALID).Index(5).Name("CONVERGENCE_VALID");
            Map(m => m.GAZERAY_ORIGIN_X).Index(6).Name("GAZERAY_ORIGIN_X");
            Map(m => m.GAZERAY_ORIGIN_Y).Index(7).Name("GAZERAY_ORIGIN_Y");
            Map(m => m.GAZERAY_ORIGIN_Z).Index(8).Name("GAZERAY_ORIGIN_Z");
            Map(m => m.GAZERAY_DIRECTION_X).Index(9).Name("GAZERAY_DIRECTION_X");
            Map(m => m.GAZERAY_DIRECTION_Y).Index(10).Name("GAZERAY_DIRECTION_Y");
            Map(m => m.GAZERAY_DIRECTION_Z).Index(11).Name("GAZERAY_DIRECTION_Z");
            Map(m => m.GAZERAY_VALID).Index(12).Name("GAZERAY_VALID");
            Map(m => m.LEFT_IS_BLINKING).Index(13).Name("LEFT_IS_BLINKING");
            Map(m => m.RIGHT_IS_BLINKING).Index(14).Name("RIGHT_IS_BLINKING");
            Map(m => m.LEFT_EYE_PUPIL_DIAMETER).Index(15).Name("LEFT_EYE_PUPIL_DIAMETER");
            Map(m => m.RIGHT_EYE_PUPIL_DIAMETER).Index(16).Name("RIGHT_EYE_PUPIL_DIAMETER");
            Map(m => m.TIMESTAMP).Index(17).Name("TIMESTAMP");
            
        }
    }
}
