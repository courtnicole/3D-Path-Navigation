namespace PathNav.PathPlanning
{
    using Dreamteck.Splines;
    using Events;
    using UnityEngine;

    public class FollowController : MonoBehaviour
    {
        [SerializeField] private SplineFollower follower;
        
        public void SubscribeToEvents()
        {
            EventManager.Subscribe<SceneControlEventArgs>(EventId.FollowPathReady, FollowPath);
        }
        
        public void UnsubscribeToEvents()
        {
            EventManager.Unsubscribe<SceneControlEventArgs>(EventId.FollowPathReady, FollowPath);
        }

        private void FollowPath(object sender, SceneControlEventArgs args)
        {
            follower.follow = true;
        }
    }
}