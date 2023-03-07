namespace PathNav.Patterns.FSM
{
    using Interaction;
    
    public class RaycastEvaluatorCanSpawn<T> : IState<T> where T : RaycastEvaluator
    {
        public void Enter(T entity)
        {
            SetSpawnPoint(entity);
        }

        public void UpdateLogic(T entity)
        {
            if (entity.ProcessInteractableCandidate())
            {
                entity.ClearSpawnPoint();
                return;
            }

            if (entity.ProcessSpawnCandidate())
            {
                SetSpawnPoint(entity);
            }

            if (entity.ShouldSelectSpawnPoint)
            {
                entity.SelectSpawnPoint();
                return;
            }

            if (entity.ShouldClearSpawnPoint)
            {
                entity.ClearSpawnPoint();
            }
        }

        public void UpdatePhysics(T entity)
        {
        }

        public void Exit(T entity)
        {
            UnsetSpawnPoint(entity);
        }

        private void SetSpawnPoint(T entity)
        {
            if (entity.SpawnPoint == entity.SpawnPointCandidate)
            {
                return;
            }

            UnsetSpawnPoint(entity);
            entity.SpawnPoint = entity.SpawnPointCandidate;
        }

        private void UnsetSpawnPoint(T entity)
        {
            ISpawnPoint spawnPoint = entity.SpawnPoint;
            
            if (spawnPoint == null)
            {
                return;
            }

            entity.SpawnPoint = null;
        }
    }
}
