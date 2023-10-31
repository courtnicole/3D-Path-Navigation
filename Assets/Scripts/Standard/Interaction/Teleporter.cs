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

        [SerializeField] private Transform startingPosition;
        private Quaternion _additionalTeleportRotation = Quaternion.identity;
        private bool _teleportToTargetObjectPivot;
        private bool _rotateToTargetObjectFront;
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

        private const float _currentFadeTime = 2.25f;
        private bool _enabled;
        #endregion

        private void Awake()
        {
            Enable();
        }

        private void Start()
        {
            if (startingPosition != null)
            {
                StartCoroutine(WaitToTeleport());
            }
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

        private IEnumerator WaitToTeleport()
        {
            yield return new WaitForSeconds(0.05f);

            Teleport(startingPosition);
        }

        public bool Teleport(Transform teleportMarker)
        {
            if (!_enabled) Enable();
            if (teleportMarker == null) return false;

            StartCoroutine(TeleportPlayer());

            return true;
        }

        private IEnumerator TeleportPlayer()
        {
            yield return new WaitForSeconds(_currentFadeTime);

            TeleportAndRotatePlayer(startingPosition);
            yield return new WaitForSeconds(_currentFadeTime);
        }

        private void MovePlayer(Vector3 targetPoint)
        {
            Vector3 playerOffset = trackingOriginTransform.position - OffsetFromTrackingOrigin;
            trackingOriginTransform.position = targetPoint + playerOffset.FlattenY();
        }

        private void TeleportAndRotatePlayer(Transform teleportTarget)
        {
            Vector3    cameraPos     = cameraTransform.position;
            Vector3    cameraForward = cameraTransform.forward;
            Vector3    originPos     = trackingOriginTransform.position;
            Quaternion originRot     = trackingOriginTransform.rotation;
            Vector3    originUp      = trackingOriginTransform.up;
            Quaternion rotateHead    = _additionalTeleportRotation;

            Quaternion headRotFrontOnFloor = Quaternion.LookRotation(Vector3.ProjectOnPlane(cameraForward, originUp), originUp);
            rotateHead = Quaternion.Inverse(headRotFrontOnFloor) * teleportTarget.rotation * rotateHead;

            Vector3    headVector = Vector3.ProjectOnPlane(cameraPos - originPos, originUp);
            Vector3    hitPos     = teleportTarget.position;
            Vector3    targetPos  = hitPos - (rotateHead * headVector);
            Quaternion targetRot  = originRot * rotateHead;

            trackingOriginTransform.position = targetPos;
            trackingOriginTransform.rotation = targetRot;
        }
    }
}