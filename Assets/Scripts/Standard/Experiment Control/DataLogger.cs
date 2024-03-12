namespace PathNav.ExperimentControl
{
    using UnityEngine;
    using LSL;

    public class DataLogger : MonoBehaviour
    {
        public static DataLogger Instance { get; private set; }
        
        private static StreamOutlet _gazeOutlet;
        private const string _gazeStreamName = "GazeStream";
        private const string _gazeStreamType = "Gaze";
        private const string _gazeStreamID = "XRData_Gaze_01";
        private const double _gazeRate = 0.00833333333; 
        private static float[] _gazeSample;
        
        private static StreamOutlet _poseOutlet;
        private const string _poseStreamName = "PoseStream";
        private const string _poseStreamType = "Pose";
        private const string _poseStreamID = "XRData_Pose_01";
        private static float[] _poseSample;

        private static StreamOutlet _navigationOutlet;
        private const string _navigationStreamName = "NavigationStream";
        private const string _navigationStreamType = "Navigation";
        private const string _navigationStreamID = "XRData_Navigation_01";
        private static float[] _navigationSample;

        private static float _userId;
        private static float _blockId;
        private static float _modelId;
        private static float _methodId;
        
        public void CreateGazeStream()
        {
            StreamInfo streamInfo = new (_gazeStreamName, _gazeStreamType, 14, _gazeRate, channel_format_t.cf_float32, _gazeStreamID);
            XMLElement channels   = streamInfo.desc().append_child("channels");

            channels.append_child("channel").append_child_value("label", "Timestamp");
            channels.append_child("channel").append_child_value("label", "ConvergenceDistance");
            channels.append_child("channel").append_child_value("label", "ConvergenceDistanceIsValid");

            channels.append_child("channel").append_child_value("label", "GazeOriginX");
            channels.append_child("channel").append_child_value("label", "GazeOriginY");
            channels.append_child("channel").append_child_value("label", "GazeOriginZ");

            channels.append_child("channel").append_child_value("label", "GazeDirectionNormalizedX");
            channels.append_child("channel").append_child_value("label", "GazeDirectionNormalizedY");
            channels.append_child("channel").append_child_value("label", "GazeDirectionNormalizedZ");

            channels.append_child("channel").append_child_value("label", "GazeRayIsValid");

            channels.append_child("channel").append_child_value("label", "LeftPupilDiameter");
            channels.append_child("channel").append_child_value("label", "RightPupilDiameter");

            channels.append_child("channel").append_child_value("label", "LeftEyeOpenness");
            channels.append_child("channel").append_child_value("label", "RightEyeOpenness");
            
            _gazeSample = new float[14];
            _gazeOutlet = new StreamOutlet(streamInfo);
        }
        
        public void CreatePoseStream()
        {
            StreamInfo streamInfo = new (_poseStreamName, _poseStreamType, 42, 0.00833333333, channel_format_t.cf_float32, _poseStreamID);
           
            XMLElement channels   = streamInfo.desc().append_child("channels");
            
            channels.append_child("channel").append_child_value("label", "Head_PosX");
            channels.append_child("channel").append_child_value("label", "Head_PosY");
            channels.append_child("channel").append_child_value("label", "Head_PosZ");
            channels.append_child("channel").append_child_value("label", "Head_RotW");
            channels.append_child("channel").append_child_value("label", "Head_RotX");
            channels.append_child("channel").append_child_value("label", "Head_RotY");
            channels.append_child("channel").append_child_value("label", "Head_RotZ");
            
            channels.append_child("channel").append_child_value("label", "LefHand_PosX");
            channels.append_child("channel").append_child_value("label", "LefHand_PosY");
            channels.append_child("channel").append_child_value("label", "LefHand_PosZ");
            channels.append_child("channel").append_child_value("label", "LefHand_RotW");
            channels.append_child("channel").append_child_value("label", "LefHand_RotX");
            channels.append_child("channel").append_child_value("label", "LefHand_RotY");
            channels.append_child("channel").append_child_value("label", "LefHand_RotZ");
            
            channels.append_child("channel").append_child_value("label", "RightHand_PosX");
            channels.append_child("channel").append_child_value("label", "RightHand_PosY");
            channels.append_child("channel").append_child_value("label", "RightHand_PosZ");
            channels.append_child("channel").append_child_value("label", "RightHand_RotW");
            channels.append_child("channel").append_child_value("label", "RightHand_RotX");
            channels.append_child("channel").append_child_value("label", "RightHand_RotY");
            channels.append_child("channel").append_child_value("label", "RightHand_RotZ");
            
            channels.append_child("channel").append_child_value("label", "Tracked_Head_PosX");
            channels.append_child("channel").append_child_value("label", "Tracked_Head_PosY");
            channels.append_child("channel").append_child_value("label", "Tracked_Head_PosZ");
            channels.append_child("channel").append_child_value("label", "Tracked_Head_RotW");
            channels.append_child("channel").append_child_value("label", "Tracked_Head_RotX");
            channels.append_child("channel").append_child_value("label", "Tracked_Head_RotY");
            channels.append_child("channel").append_child_value("label", "Tracked_Head_RotZ");
            
            channels.append_child("channel").append_child_value("label", "Tracked_LefHand_PosX");
            channels.append_child("channel").append_child_value("label", "Tracked_LefHand_PosY");
            channels.append_child("channel").append_child_value("label", "Tracked_LefHand_PosZ");
            channels.append_child("channel").append_child_value("label", "Tracked_LefHand_RotW");
            channels.append_child("channel").append_child_value("label", "Tracked_LefHand_RotX");
            channels.append_child("channel").append_child_value("label", "Tracked_LefHand_RotY");
            channels.append_child("channel").append_child_value("label", "Tracked_LefHand_RotZ");
            
            channels.append_child("channel").append_child_value("label", "Tracked_RightHand_PosX");
            channels.append_child("channel").append_child_value("label", "Tracked_RightHand_PosY");
            channels.append_child("channel").append_child_value("label", "Tracked_RightHand_PosZ");
            channels.append_child("channel").append_child_value("label", "Tracked_RightHand_RotW");
            channels.append_child("channel").append_child_value("label", "Tracked_RightHand_RotX");
            channels.append_child("channel").append_child_value("label", "Tracked_RightHand_RotY");
            channels.append_child("channel").append_child_value("label", "Tracked_RightHand_RotZ");
            _poseSample = new float[42];
            _poseOutlet = new StreamOutlet(streamInfo);
        }

        public void CreateNavigationStream()
        {
            StreamInfo streamInfo = new (_navigationStreamName, _navigationStreamType, 7, 0.00833333333, channel_format_t.cf_float32, _navigationStreamID);
            
            XMLElement channels = streamInfo.desc().append_child("channels");
            
            channels.append_child("channel").append_child_value("label", "speed");
            channels.append_child("channel").append_child_value("label", "spline_position");
            channels.append_child("channel").append_child_value("label", "spline_percent");
        }
    }
}