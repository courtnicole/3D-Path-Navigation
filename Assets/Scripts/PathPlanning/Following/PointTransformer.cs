using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace PathNav.PathPlanning
{
    using Dreamteck.Splines;
    using System;

    public class PointTransformer 
    {
        public enum EditSpace { World, Transform, Spline }
        public EditSpace editSpace = EditSpace.World;
        public Vector3 scale = Vector3.one, offset = Vector3.zero;
        protected Quaternion rotation = Quaternion.identity;
        protected Vector3 origin = Vector3.zero;
        protected SplinePoint[] originalPoints = Array.Empty<SplinePoint>();
        protected SplinePoint[] localPoints = Array.Empty<SplinePoint>();

        private Matrix4x4 matrix = new Matrix4x4();
        private Matrix4x4 inverseMatrix = new Matrix4x4();
        private bool _unapplied = true;
        SplineSample evalResult = new SplineSample();
    }
}
