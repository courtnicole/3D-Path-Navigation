
namespace PathNav.ExperimentControl
{
    public class SceneDataFormat
    {
        public int    ID               { get; set; }	
        public string BLOCK_ID         { get; set; }	
        public string TRIAL_TYPE       { get; set; }	
        public int    TRIAL_ID         { get; set; }	
        public string SCENE_ID         { get; set; }	
        public string METHOD           { get; set; }	
        public string MODEL            { get; set; }	
        public int    ACTIONS_TOTAL    { get; set; }	
        public int    ACTIONS_EDIT     { get; set; }	
        public int    ACTIONS_CREATE   { get; set; }	
        public float  TASK_TIME_TOTAL  { get; set; }	
        public float  TASK_TIME_EDIT   { get; set; }	
        public float  TASK_TIME_CREATE { get; set; }	
        public int    SEQ_SCORE        { get; set; }	
        public int    DISCOMFORT_SCORE { get; set; }	
    }
}
