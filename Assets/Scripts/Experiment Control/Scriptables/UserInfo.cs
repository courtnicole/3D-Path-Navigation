namespace PathNav.ExperimentControl
{
    using System;
    
    public struct UserInfo
    {
        public UserInfo(int id, float height)
        {
            UserId         = id;
            BlockId        = id % 4;
            Height         = height;
            DataFile       = "User_" + UserId + "_" + DateTime.Now.ToString("yyyy_MM_dd_HH_mm") + "_Summary.csv";
            ActionFile     = "User_" + UserId + "_" + DateTime.Now.ToString("yyyy_MM_dd_HH_mm") + "_Actions.csv";
            NavigationFile = "User_" + UserId + "_" + DateTime.Now.ToString("yyyy_MM_dd_HH_mm") + "_Navigation.csv";
            PoseFile       = "User_" + UserId + "_" + DateTime.Now.ToString("yyyy_MM_dd_HH_mm") + "_Pose.csv";
            GazeFile       = "User_" + UserId + "_" + DateTime.Now.ToString("yyyy_MM_dd_HH_mm") + "_Gaze.csv";
            LuminanceFile  = "User_" + UserId + "_" + DateTime.Now.ToString("yyyy_MM_dd_HH_mm") + "_Luminance.csv";
        }
        public int      UserId   { get; private set; }
        
        public float Height { get; private set; }

        public int BlockId { get; private set; }

        public string DataFile { get; private set; }
        
        public string ActionFile     { get; private set; }
        public string NavigationFile { get; private set; }
        public string PoseFile { get; private set; }
        public string GazeFile { get; private set; }
        
        public string LuminanceFile { get; private set; }
    }
}
