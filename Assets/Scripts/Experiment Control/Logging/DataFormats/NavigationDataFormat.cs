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
        
        public double TIMESTAMP { get; set; }
    }
}
