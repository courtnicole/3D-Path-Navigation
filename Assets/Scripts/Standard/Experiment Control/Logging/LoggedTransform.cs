namespace DataLogging
{
    using System.Globalization;
    using UnityEngine;

    /// <summary>
    /// Represents a <c>LoggedTransform</c> that is used to log the position and rotation of a <see cref="UnityEngine.Transform"/> object
    /// </summary>
    /// <remarks>
    /// We use a struct here to avoid the overhead of a class.
    /// It is marked as serializable so that it can be used in the Unity inspector.
    /// </remarks>>
    [System.Serializable]
    public struct LoggedTransform
    {
        /// <summary>
        /// Constructs a LoggedTransform object
        /// </summary>
        /// <param name="t">The <see cref="UnityEngine.Transform"/> object to log</param>
        /// <param name="name">The <see cref="System.String"/> name of the transform</param>
        /// <param name="logPose">The <see cref="System.Boolean"/> value indicating whether to log the position</param>
        /// <param name="logRot">The <see cref="System.Boolean"/> value indicating whether to log the rotation</param>
        public LoggedTransform(Transform t, string name, bool logPose, bool logRot)
        {
            transform   = t;
            logPosition = logPose;
            logRotation = logRot;
            nameId      = name;

            if (string.IsNullOrEmpty(name)) nameId = t.name + "_" + t.GetInstanceID();
        }

        public Transform transform;
        public string nameId;
        public bool logPosition;
        public bool logRotation;
    }
    
    /// <summary>
    /// Represents a <c>TransformPosition</c> that is used to log the position Vector of a <see cref="UnityEngine.Transform"/> object.
    /// You should not use this class directly. Instead, use the <see cref="LoggedTransform"/> class and set the <c>logPosition</c> field to <c>true</c>.
    /// </summary>
    /// <remarks>
    /// This struct is not marked as serializable because it is not intended to be used in the Unity inspector.
    /// It is readonly to prevent accidental modification of the struct fields at runtime.
    /// This also allows it to be written to a file without additional overhead.
    /// </remarks>>
    public readonly struct TransformPosition
    {
        public TransformPosition(Transform t, string name)
        {
            _transform   = t;
            _transformId = name;
        }

        private readonly Transform _transform;
        private readonly string _transformId;

        public Field PoseX => new Field($"{_transformId}_x_pose", _transform.position.x.ToString(CultureInfo.InvariantCulture));
        public Field PoseY => new Field($"{_transformId}_y_pose", _transform.position.y.ToString(CultureInfo.InvariantCulture));
        public Field PoseZ => new Field($"{_transformId}_z_pose", _transform.position.z.ToString(CultureInfo.InvariantCulture));
    }

    /// <summary>
    /// Represents a <c>TransformRotation</c> that is used to log the rotation Quaternion of a <see cref="UnityEngine.Transform"/> object.
    /// You should not use this class directly. Instead, use the <see cref="LoggedTransform"/> class and set the <c>logRotation</c> field to <c>true</c>.
    /// </summary>
    /// <remarks>
    /// We record the rotation as a Quaternion because it is more efficient than a Vector3, and the conversion is trivial.
    /// This struct is not marked as serializable because it is not intended to be used in the Unity inspector.
    /// It is readonly to prevent accidental modification of the struct fields at runtime.
    /// This also allows it to be written to a file without additional overhead.
    /// </remarks>>
    public readonly struct TransformRotation
    {
        public TransformRotation(Transform t, string name)
        {
            _transform   = t;
            _transformId = name;
        }

        private readonly Transform _transform;
        private readonly string _transformId;

        public Field RotX => new Field($"{_transformId}_x_rot", _transform.rotation.x.ToString(CultureInfo.InvariantCulture));
        public Field RotY => new Field($"{_transformId}_y_rot", _transform.rotation.y.ToString(CultureInfo.InvariantCulture));
        public Field RotZ => new Field($"{_transformId}_z_rot", _transform.rotation.z.ToString(CultureInfo.InvariantCulture));
        public Field RotW => new Field($"{_transformId}_w_rot", _transform.rotation.w.ToString(CultureInfo.InvariantCulture));
    }
}