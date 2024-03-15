namespace PathNav.Interaction
{
    using Extensions;
    using System.Collections;
    using UnityEngine;

    
    
    public class Teleporter : MonoBehaviour
    {
        #region Inspector Variables
        [SerializeField] private Transform trackingOriginTransform;
        [SerializeField] private Transform cameraTransform;
        
        private Quaternion _additionalTeleportRotation = Quaternion.identity;
        #endregion

        #region Local Variables
        //Used to find the "true" y = 0 value of the Player, based on their height + compensating for uneven terrain
        //From SteamVR Player
        private Vector3 OffsetFromTrackingOrigin
        {
            get
            {
                Transform hmd = cameraTransform;
                if (!hmd) return trackingOriginTransform.position;

                Vector3 position = trackingOriginTransform.position;
                return position + Vector3.ProjectOnPlane(hmd.position - position, trackingOriginTransform.up);
            }
        }
        
        private bool _enabled;
        #endregion

        private void Awake()
        {
            Enable();
        }

        private void Enable()
        {
            if (trackingOriginTransform is null)
            {
                if (!Utility.GetCameraRigTransform(out trackingOriginTransform))
                {
                    enabled = false;
                    return;
                }
            }

            if (cameraTransform is null)
            {
                if (!Utility.GetCameraTransform(out cameraTransform))
                {
                    enabled = false;
                    return;
                }
            }

            _enabled = true;
        }

        public bool Teleport(Transform teleportMarker)
        {
            if (!_enabled) Enable();
            if (teleportMarker == null) return false;

            StartCoroutine(TeleportPlayer(teleportMarker));

            return true;
        }

        private IEnumerator TeleportPlayer(Transform teleportMarker)
        {
            yield return null;
            TeleportAndRotatePlayer(teleportMarker);
            yield return null;
        }

        private void MovePlayer(Vector3 targetPoint)
        {
            Vector3 playerOffset = trackingOriginTransform.position - OffsetFromTrackingOrigin;
            trackingOriginTransform.position = targetPoint + playerOffset.FlattenY();
        }

        //Based on SteamVR Teleporter
        private void TeleportAndRotatePlayer(Transform teleportMarker)
        {
            Vector3    cameraForward = cameraTransform.forward;
            Quaternion originRot     = trackingOriginTransform.rotation;
            Vector3    originUp      = trackingOriginTransform.up;
            Quaternion rotateHead    = _additionalTeleportRotation;

            Quaternion headRotationOnGround = Quaternion.LookRotation(Vector3.ProjectOnPlane(cameraForward, originUp), originUp);
            rotateHead = Quaternion.Inverse(headRotationOnGround) * teleportMarker.rotation * rotateHead;

            Vector3    headVector     = OffsetFromTrackingOrigin;
            Vector3    targetPosition = teleportMarker.position - (rotateHead * headVector);
            Quaternion targetRotation = originRot * rotateHead;
            
            targetRotation = Quaternion.Euler(0, targetRotation.eulerAngles.y, 0);

            trackingOriginTransform.position = targetPosition;
            trackingOriginTransform.rotation = targetRotation;
        }
    }
}