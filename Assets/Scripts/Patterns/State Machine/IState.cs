namespace PathNav.Patterns.FSM
{
    public interface IState <T>
    {
        public void Enter(T entity);
        public void UpdateLogic(T entity);
        public void UpdatePhysics(T entity);
        public void Exit(T entity);
    }
}