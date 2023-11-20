namespace PathNav.ExperimentControl
{
    using System;
    using UnityEngine;
    
    [CreateAssetMenu(fileName = "UserInfo", menuName = "Scriptables/Standard/UserInfo", order = 100)]
    public class UserInfo : ScriptableObject
    {
        public void Initialize(int id)
        {
            UserId   = id;
            BlockId  = id % 4;
            DataFile = "User_" + UserId + "_" + DateTime.Now.ToString("yyyy_MM_dd_HH_mm_ss") + ".csv";
        }

        public int UserId { get; private set; }

        public int BlockId { get; private set; }

        public string DataFile { get; private set; }
    }
}
