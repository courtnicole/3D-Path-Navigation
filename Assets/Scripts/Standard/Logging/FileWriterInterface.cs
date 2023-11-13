namespace DataLogging
{
    using PathNav.ExperimentControl;
    using System;
    using System.Collections.Generic;
    using System.Reflection;
    using UnityEngine;

    /// <summary>
    /// This class is used to interface with the <see cref="FileWriter"/> class.
    /// Handles the creation of the <see cref="FileWriter"/> object and the logging of data.
    /// </summary>
    /// <remarks>
    /// The class is static so that it can be accessed from anywhere in the project.
    /// It cannot need to be attached to a game object, as it does not inherit from <see cref="MonoBehaviour"/>.
    /// </remarks>
    public static class FileWriterInterface
    {
        /// <summary>
        /// A reference to the <see cref="FileWriter"/> object
        /// used to write to the log file
        /// </summary>
        private static FileWriter _logger;

        /// <summary>
        /// Sets up a log file for the game object
        /// </summary>
        /// <param name="sender">The <see cref="UnityEngine.GameObject"/> The game object to set the log file for</param>
        /// <param name="useAutomaticQueuing">The <see cref="System.Boolean"/> Flag to indicate whether to queue automatically</param>
        /// <param name="userInfo"></param>
        /// <param name="logDirectory">The <see cref="System.String"/> The directory to use for the log file</param>
        /// <param name="logPrefix">The <see cref="System.String"/> A prefix to use for the log file</param>
        /// <returns>The <see cref="System.Boolean"/> Determines whether the log file was successfully set up</returns>
        public static bool SetupLogFile(GameObject sender, UserInfo userInfo, string logDirectory = "Data", bool useAutomaticQueuing = false )
        {
            _logger = sender.AddComponent<FileWriter>();

             logDirectory = $"{logDirectory}";
            _logger.logDirectory = logDirectory;
            _logger.logName      = userInfo.DataFile;

            _logger.queueOnLateUpdate = useAutomaticQueuing;

            return _logger != null;
        }

        /// <summary>
        /// Initializes the logging process
        /// </summary>
        /// <returns>The <see cref="System.Boolean"/> Determines whether the logging process was successfully initialized</returns>
        public static bool InitializeLogging()
        {
            if (_logger == null) return false;

            _logger.Initialize();
            return true;
        }

        /// <summary>
        /// Adds a logged item of type <typeparamref name="T"/> to the log
        /// </summary>
        /// <typeparam name="T">The <see cref="System.Type"/> type of the item to log</typeparam>
        /// <param name="itemId">The <see cref="System.String"/> Unique ID of the item to log</param>
        public static void AddLoggedItem<T>(string itemId)
        {
            if (_logger == null)
            {
                Debug.LogError("Logger is null!");
                throw new Exception("Logger is null!");
            }

            _logger.fields.Add(itemId);
        }

        /// <summary>
        /// Adds a <see cref="TransformPosition"/> item to the log
        /// </summary>
        /// <param name="item">The <see cref="TransformPosition"/> item to log</param>
        public static void AddLoggedItem(TransformPosition item)
        {
            if (_logger == null)
            {
                Debug.LogError("Logger is null!");
                throw new Exception("Logger is null!");
            }

            _logger.fields.Add(item.PoseX.LogId);
            _logger.fields.Add(item.PoseY.LogId);
            _logger.fields.Add(item.PoseZ.LogId);
        }

        /// <summary>
        /// Adds a <see cref="TransformRotation"/> item to the log
        /// </summary>
        /// <param name="item">The <see cref="TransformRotation"/> The item to log</param>
        public static void AddLoggedItem(TransformRotation item)
        {
            if (_logger == null)
            {
                Debug.LogError("Logger is null!");
                throw new Exception("Logger is null!");
            }

            _logger.fields.Add(item.RotX.LogId);
            _logger.fields.Add(item.RotY.LogId);
            _logger.fields.Add(item.RotZ.LogId);
            _logger.fields.Add(item.RotW.LogId);
        }

        /// <summary>
        /// Adds a struct item to the log
        /// </summary>
        /// <typeparam name="T">The <see cref="System.Type"/> The type of the item to log</typeparam>
        /// <param name="item">The <see cref="System.Object"/> The item to log</param>
        public static void AddLoggedItem<T>(T item) where T : struct
        {
            if (_logger == null)
            {
                Debug.LogError("Logger is null!");
                throw new Exception("Logger is null!");
            }

            foreach (PropertyInfo field in typeof(T).GetProperties())
            {
                Debug.Log(field.Name);
                _logger.fields.Add(field.Name);
            }
        }
        
        /// <summary>
        /// Records the data of the item of type <typeparamref name="T"/>
        /// </summary>
        /// <typeparam name="T">The <see cref="System.Type"/> The type of the item to record</typeparam>
        /// <param name="item">The <see cref="System.Object"/> The item to record</param>
        /// <param name="id">The <see cref="System.String"/> The ID of the item to record</param>
        public static void RecordData<T>(string id, T item)
        {
            if (_logger == null)
            {
                Debug.LogError("Logger is null!");
                throw new Exception("Logger is null!");
            }

            _logger.UpdateField(id, item.ToString());
        }

        /// <summary>
        /// Records the data of a <see cref="TransformPosition"/> item
        /// </summary>
        /// <param name="item">The <see cref="TransformPosition"/> The item to record</param>
        public static void RecordData(TransformPosition item)
        {
            if (_logger == null)
            {
                Debug.LogError("Logger is null!");
                throw new Exception("Logger is null!");
            }

            _logger.UpdateField(item.PoseX.LogId, item.PoseX.LogData);
            _logger.UpdateField(item.PoseY.LogId, item.PoseY.LogData);
            _logger.UpdateField(item.PoseZ.LogId, item.PoseZ.LogData);
        }

        /// <summary>
        /// Records the data of a <see cref="TransformRotation"/> item
        /// </summary>
        /// <param name="item">The <see cref="TransformRotation"/> The item to record</param>
        public static void RecordData(TransformRotation item)
        {
            if (_logger == null)
            {
                Debug.LogError("Logger is null!");
                throw new Exception("Logger is null!");
            }

            _logger.UpdateField(item.RotX.LogId, item.RotX.LogData);
            _logger.UpdateField(item.RotY.LogId, item.RotY.LogData);
            _logger.UpdateField(item.RotZ.LogId, item.RotZ.LogData);
            _logger.UpdateField(item.RotW.LogId, item.RotW.LogData);
        }

        /// <summary>
        /// Records the data of a struct item
        /// </summary>
        public static void RecordData<T>(T item) where T : struct
        {
            if (_logger == null)
            {
                Debug.LogError("Logger is null!");
                throw new Exception("Logger is null!");
            }

            foreach (PropertyInfo field in typeof(T).GetProperties())
            {
                _logger.UpdateField(field.Name, field.GetValue(item).ToString());
            }
        }
        
        /// <summary>
        /// Sends logged data to the queue
        /// </summary>
        public static void WriteRecordedData()
        {
            if (_logger == null)
            {
                Debug.LogError("Logger is null!");
                throw new Exception("Logger is null!");
            }

            _logger.Queue();
        }

        /// <summary>
        /// Writes the recorded data to the log file
        /// </summary>
        public static List<string> DebugGetFields() => _logger.fields;
    }
}