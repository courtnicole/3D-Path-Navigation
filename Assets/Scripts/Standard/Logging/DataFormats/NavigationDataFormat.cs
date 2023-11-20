namespace PathNav.ExperimentControl
{
    using System;

    public class NavigationDataFormat 
    {
        public int      ID        { get; set; }	
        public int      BLOCK_ID  { get; set; }	
        public string   MODEL     { get; set; }
        public string   METHOD    { get; set; }	
        public float    SPEED     { get; set; }
        public string   POSITION  { get; set; }
        public string   ROTATION  { get; set; }
        public DateTime TIMESTAMP { get; set; }
    }
}
