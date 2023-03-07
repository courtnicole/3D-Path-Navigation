namespace PathNav.Patterns.FSM
{
    public class HierarchicalStateMachine<T> 
    {
        private T _owner;

        public bool IsConfigured => CurrentState != null && PreviousState != null;

        public IState<T> CurrentState { get; private set; }
        public IState<T> PreviousState { get; private set; }

        public void ChangeState(IState<T> newState)
        {
            PreviousState = CurrentState;

            CurrentState?.Exit(_owner);
            CurrentState = newState;
            CurrentState?.Enter(_owner);
        }
    }
}
