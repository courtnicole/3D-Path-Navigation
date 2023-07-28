/*
 * Asynchronous CSV Writer
 * Jerald Thomas 06/18/2019
 *
 * Demo and modifications
 * Courtney Hutton Pospick
 * 09/22/2020
 *
 */
namespace DataLogging
{
    using System.Collections.Generic;
    using System.IO;
    using System.Threading;
    using UnityEngine;

    public class FileWriter : MonoBehaviour
    {
        [Tooltip("Log directory relative to the project directory")]
        public string logDirectory = "Data";

        [Tooltip("Name of the log file")] public string logName = "CsvDemo";

        [Tooltip("Character that separates values")]
        public char delimiter = ',';

        [Tooltip("If true, all values will enqueue to the write queue on LateUpdate")]
        public bool queueOnLateUpdate;

        [Tooltip("If true, all values will be set to defaultValue after queueing")]
        private const bool _flushOnQueue = true;

        [Tooltip("List of fields to log")] public List<string> fields = new List<string>();

        public string fileType = ".csv";
        private string _value = "N/A";

        private Dictionary<string, string> _values = new Dictionary<string, string>();

        private Thread _writeThread;
        private readonly object _writeLock = new object();
        private volatile Queue<string> _writeQueue = new Queue<string>();
        private volatile bool _threadRunning = true;
        private volatile StreamWriter _writer;

        public void Initialize()
        {
            if (logDirectory == string.Empty)
                logDirectory = Application.dataPath;
            else
                logDirectory = Application.dataPath + "/" + logDirectory;

            if (!Directory.Exists(logDirectory))
            {
                Directory.CreateDirectory(logDirectory);
            }

            string filePath = logDirectory + "/" + logName + fileType;

            if (!File.Exists(filePath))
            {
                string header = "";

                foreach (string field in fields)
                {
                    _values.Add(field, _value);
                    header += field + delimiter;
                }

                _writeQueue.Enqueue(header);
            }

            _writer      = new StreamWriter(filePath, true);
            _writeThread = new Thread(ThreadJob);
            _writeThread.Start();
        }

        private void LateUpdate()
        {
            if (queueOnLateUpdate)
                Queue();
        }

        private void OnDestroy()
        {
            EndJob();
        }

        public void UpdateField(string field, string value)
        {
            if (fields.Contains(field))
            {
                _values[field] = value;
            }
            else
            {
                Debug.LogWarning("FileWriter: Log <" + logName + "> does not contain field <" + field + ">.");
            }
        }

        public void Queue()
        {
            string line = "";

            foreach (string field in fields)
            {
                line += _values[field] + delimiter;
            }

            lock (_writeLock)
            {
                _writeQueue.Enqueue(line);
            }

            if (_flushOnQueue)
                Flush();
        }

        private void Flush()
        {
            foreach (string field in fields)
            {
                _values[field] = _value;
            }
        }

        private void ThreadJob()
        {
            while (_threadRunning)
            {
                while (_writeQueue.Count > 0)
                {
                    lock (_writeLock)
                    {
                        _writer.WriteLine(_writeQueue.Dequeue());
                    }
                }
            }
        }

        private void EndJob()
        {
            lock (_writeLock)
            {
                _threadRunning = false;
                if (_writer == null) return;

                _writer.Close();
            }
        }
    }
}