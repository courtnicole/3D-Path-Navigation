namespace PathNav
{
    using Interaction;
    using UnityEngine;

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
