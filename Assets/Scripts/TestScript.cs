namespace PathNav
{
    using Interaction;
    using System.Collections;
    using System.Collections.Generic;
    using UnityEngine;
    using Valve.VR.InteractionSystem;

    public class TestScript : MonoBehaviour
    {
        public Teleporter Teleporter;
        public Transform TargetPoint;

        public void TestTeleport()
        {
            Teleporter.Teleport(TargetPoint);
        }
    }
}
