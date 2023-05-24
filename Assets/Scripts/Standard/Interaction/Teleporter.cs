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
        private Vector3 _teleportPoint;
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

            _teleportPoint = teleportMarker.position;
            
            StartCoroutine(TeleportPlayer());
            
            return true;
        }

        private IEnumerator TeleportPlayer()
        {
            FadeOut();
            yield return new WaitForSeconds(_currentFadeTime);
            MovePlayer(_teleportPoint);
            yield return new WaitForSeconds(_currentFadeTime);
            FadeIn();
        }

        private static void FadeOut()
        {
            //SteamVR_Fade.View(Color.black, _currentFadeTime);
        }

        private static void FadeIn()
        {
            //SteamVR_Fade.View(Color.clear, _currentFadeTime);
        }

        public void MoveAndRotate(Vector3 move, Vector3 look)
        {
            StartCoroutine(DoMoveAndRotate(move, look));
        }

        private void MovePlayer(Vector3 targetPoint)
        {
            Vector3 playerOffset = trackingOriginTransform.position - OffsetFromTrackingOrigin;
            trackingOriginTransform.position = targetPoint + playerOffset.FlattenY();
        }

        private IEnumerator DoMoveAndRotate(Vector3 teleportPosition, Vector3 look)
        {
            Vector3 trackingOriginPosition = trackingOriginTransform.position;
            Vector3 playerOffset = trackingOriginPosition - OffsetFromTrackingOrigin;
            trackingOriginPosition = teleportPosition + playerOffset.FlattenY();

            yield return new WaitForEndOfFrame();

            float angle = Vector3.Angle(cameraTransform.forward, look);
            Vector3 playerFeetOffset = trackingOriginPosition - OffsetFromTrackingOrigin;
            trackingOriginPosition -= playerFeetOffset;
            trackingOriginTransform.Rotate(Vector3.up, angle);
            playerFeetOffset = Quaternion.Euler(0.0f, angle, 0.0f) * playerFeetOffset;
            trackingOriginPosition += playerFeetOffset;
            trackingOriginTransform.position = trackingOriginPosition;
        }
    }
}
