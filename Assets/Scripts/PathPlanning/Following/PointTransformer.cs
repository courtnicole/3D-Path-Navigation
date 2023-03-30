namespace PathNav.PathPlanning
{
    using Dreamteck.Splines;
    using UnityEngine;

    public class PointTransformer
    {
        private Vector3 _scale;
        private SplinePoint[] _localPoints;

        private Matrix4x4 _matrix;
        private Matrix4x4 _inverseMatrix;
        
        private Vector3 PointCenter
        {
            get
            {
                Vector3 average = Vector3.zero;
                if (_localPoints.Length == 0) return average;

                for (int i = 0; i < _localPoints.Length; i++)
                {
                    average += _localPoints[i].position;
                }

                return average / _localPoints.Length;
            }
        }
        
        public PointTransformer(SplinePoint[] points, Vector3 transformPosition, Quaternion transformRotation, Vector3 transformScale)
        {
            _localPoints = points;
            _scale       = transformScale;
            _matrix.SetTRS(PointCenter + transformPosition, transformRotation, _scale);
            _inverseMatrix = _matrix.inverse;
        }

        public void TransformPoint(ref SplinePoint point, bool normals = true, bool tangents = true, bool size = false)
        {
            if (tangents)
            {
                point.position = TransformPosition(point.position);
                point.tangent  = TransformPosition(point.tangent);
                point.tangent2 = TransformPosition(point.tangent2);
            }
            else
            {
                point.SetPosition(TransformPosition(point.position));
            }

            if (normals) point.normal = TransformDirection(point.normal).normalized;

            if (size)
            {
                float avg = (_scale.x + _scale.y + _scale.z) / 3f;
                point.size *= avg;
            }
        }

        public void InverseTransformPoint(ref SplinePoint point, bool normals = true, bool tangents = true, bool size = false)
        {
            if (tangents)
            {
                point.position = InverseTransformPosition(point.position);
                point.tangent  = InverseTransformPosition(point.tangent);
                point.tangent2 = InverseTransformPosition(point.tangent2);
            }
            else point.SetPosition(TransformPosition(point.position));

            if (normals) point.normal = InverseTransformDirection(point.normal).normalized;

            if (size)
            {
                float avg = (_scale.x + _scale.y + _scale.z) / 3f;
                point.size /= avg;
            }
        }
        
        private Vector3 TransformPosition(Vector3 position) => _matrix.MultiplyPoint3x4(position);
        private Vector3 InverseTransformPosition(Vector3 position) => _inverseMatrix.MultiplyPoint3x4(position);

        private Vector3 TransformDirection(Vector3 direction) => _matrix.MultiplyVector(direction);
        private Vector3 InverseTransformDirection(Vector3 direction) => _inverseMatrix.MultiplyVector(direction);
    }
}