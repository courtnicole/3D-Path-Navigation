namespace PathNav.ExperimentControl
{
    using System;
    
    public struct UserInfo
    {
        public UserInfo(int id)
        {
            UserId     = id;
            BlockId    = id % 4;
            DataFile   = "User_" + UserId + "_" + DateTime.Now.ToString("yyyy_MM_dd_HH_mm") + ".csv";
            ActionFile = "User_" + UserId + "_" + DateTime.Now.ToString("yyyy_MM_dd_HH_mm") + "_Actions.csv";
        }
        public int      UserId   { get; private set; }

        public int BlockId { get; private set; }

        public string DataFile { get; private set; }
        
        public string ActionFile { get; private set; }
    }
}
