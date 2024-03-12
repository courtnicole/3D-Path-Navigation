namespace PathNav.ExperimentControl
{
    public class GazeDataFormat 
    {
        public int    ID       { get; set; }	
        public int    BLOCK_ID { get; set; }	
        public string MODEL    { get; set; }
        public string METHOD   { get; set; }
        
        public float CONVERGENCE_DISTANCE { get; set; }
        public bool  CONVERGENCE_VALID    { get; set; }
        
        public float GAZERAY_ORIGIN_X { get; set; }
        public float GAZERAY_ORIGIN_Y { get; set; }
        public float GAZERAY_ORIGIN_Z { get; set; }
        
        public float GAZERAY_DIRECTION_X { get; set; }
        public float GAZERAY_DIRECTION_Y { get; set; }
        public float GAZERAY_DIRECTION_Z { get; set; }
        
        public bool GAZERAY_VALID { get; set; }
        
        public bool LEFT_IS_BLINKING { get; set; }
        public bool RIGHT_IS_BLINKING { get; set; }
        
        public float LEFT_EYE_PUPIL_DIAMETER  { get; set; }
        public float RIGHT_EYE_PUPIL_DIAMETER { get; set; }
    }
}
