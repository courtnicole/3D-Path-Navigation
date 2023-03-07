namespace PathNav.Interaction
{
    using UnityEngine;
    
    public interface ICollider
    {
        public void TriggerEntered(Collider other);
        public void TriggerExited(Collider other);
        public void TriggerStayed(Collider other);
        
        
    }
}
