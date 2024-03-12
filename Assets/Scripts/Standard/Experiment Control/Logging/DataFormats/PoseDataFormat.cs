namespace PathNav.ExperimentControl
{
    using System;

    public class PoseDataFormat 
    {
        public int    ID              { get; set; }	
        public int    BLOCK_ID        { get; set; }	
        public string MODEL           { get; set; }
        public string METHOD          { get; set; }
        
        public float  HEAD_POSITION_X { get; set; }
        public float  HEAD_POSITION_Y { get; set; }
        public float  HEAD_POSITION_Z { get; set; }
        
        public float  HEAD_ROTATION_X { get; set; }
        public float  HEAD_ROTATION_Y { get; set; }
        public float  HEAD_ROTATION_Z { get; set; }
        public float  HEAD_ROTATION_W { get; set; }
        
        public float  LEFT_POSITION_X { get; set; }
        public float  LEFT_POSITION_Y { get; set; }
        public float  LEFT_POSITION_Z { get; set; }
        public float  LEFT_ROTATION_X { get; set; }
        public float  LEFT_ROTATION_Y { get; set; }
        public float  LEFT_ROTATION_Z { get; set; }
        public float  LEFT_ROTATION_W { get; set; }
        
        public float  RIGHT_POSITION_X { get; set; }
        public float  RIGHT_POSITION_Y { get; set; }
        public float  RIGHT_POSITION_Z { get; set; }
        public float  RIGHT_ROTATION_X { get; set; }
        public float  RIGHT_ROTATION_Y { get; set; }
        public float  RIGHT_ROTATION_Z { get; set; }
        public float  RIGHT_ROTATION_W { get; set; }
        
        public float  TRACKED_HEAD_POSITION_X { get; set; }
        public float  TRACKED_HEAD_POSITION_Y { get; set; }
        public float  TRACKED_HEAD_POSITION_Z { get; set; }
        public float  TRACKED_HEAD_ROTATION_X { get; set; }
        public float  TRACKED_HEAD_ROTATION_Y { get; set; }
        public float  TRACKED_HEAD_ROTATION_Z { get; set; }
        public float  TRACKED_HEAD_ROTATION_W { get; set; }
        
        public float  TRACKED_LEFT_POSITION_X { get; set; }
        public float  TRACKED_LEFT_POSITION_Y { get; set; }
        public float  TRACKED_LEFT_POSITION_Z { get; set; }
        public float  TRACKED_LEFT_ROTATION_X { get; set; }
        public float  TRACKED_LEFT_ROTATION_Y { get; set; }
        public float  TRACKED_LEFT_ROTATION_Z { get; set; }
        public float  TRACKED_LEFT_ROTATION_W { get; set; }
        
        public float TRACKED_RIGHT_POSITION_X { get; set; }
        public float TRACKED_RIGHT_POSITION_Y { get; set; }
        public float TRACKED_RIGHT_POSITION_Z { get; set; }
        public float TRACKED_RIGHT_ROTATION_X { get; set; }
        public float TRACKED_RIGHT_ROTATION_Y { get; set; }
        public float TRACKED_RIGHT_ROTATION_Z { get; set; }
        public float TRACKED_RIGHT_ROTATION_W { get; set; }
        
        public DateTime TIMESTAMP { get; set; }
    }
}
