namespace PathNav.ExperimentControl
{
    public class LuminanceDataFormat
    {
        public int    ID        { get; set; }
        public int    BLOCK_ID  { get; set; }
        public string MODEL     { get; set; }
        public string METHOD    { get; set; }
        public float  LUMINANCE { get; set; }
        public double TIMESTAMP { get; set; }
    }
}