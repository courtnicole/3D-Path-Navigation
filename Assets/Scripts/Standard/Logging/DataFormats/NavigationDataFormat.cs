namespace PathNav.ExperimentControl
{
    using System;

    public class NavigationDataFormat 
    {
        public int    ID              { get; set; }	
        public int    BLOCK_ID        { get; set; }	
        public string MODEL           { get; set; }
        public string METHOD          { get; set; }	
        public float  SPEED           { get; set; }
        public double SPLINE_PERCENT  { get; set; }
        public string SPLINE_POSITION { get; set; }
        public string HEAD_POSITION        { get; set; }
        public string HEAD_ROTATION        { get; set; }
        
        public string LEFT_POSITION { get; set; }
        public string LEFT_ROTATION { get; set; }
        
        public string RIGHT_POSITION { get; set; }
        public string RIGHT_ROTATION { get; set; }
        
        public string TRACKED_HEAD_POSITION { get; set; }
        public string TRACKED_HEAD_ROTATION { get; set; }
        
        public string TRACKED_LEFT_POSITION { get; set; }
        public string TRACKED_LEFT_ROTATION { get; set; }
        
        public string TRACKED_RIGHT_POSITION { get; set; }
        public string TRACKED_RIGHT_ROTATION { get; set; }

        
        public DateTime TIMESTAMP { get; set; }
    }
}
