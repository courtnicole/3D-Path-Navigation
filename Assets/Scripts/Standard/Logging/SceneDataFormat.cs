
namespace PathNav.ExperimentControl
{
    public class SceneDataFormat
    {
        

        public int    ID               { get; set; }	
        public int BLOCK_ID         { get; set; }	
        //Creation or Navigation
        public string TRIAL_TYPE       { get; set; }	
        //Trial count within this type (eg, round 1, round 2, etc)
        public int    TRIAL_ID         { get; set; }	
        //Name of the trial scene
        public string SCENE_ID         { get; set; }	
        //Technique used
        public string METHOD           { get; set; }	
        //ID of the model used (A, B, C, etc)
        public string MODEL            { get; set; }	
        public int    ACTIONS_TOTAL    { get; set; }	
        public int    ACTIONS_EDIT     { get; set; }	
        public float  TASK_TIME_TOTAL  { get; set; }	
        public float  TASK_TIME_EDIT   { get; set; }	
        public float  TASK_TIME_CREATE { get; set; }	
        public int    SEQ_SCORE        { get; set; }	
        public int    DISCOMFORT_SCORE { get; set; }	
    }
}
