namespace PathNav.Patterns.FSM
{
    public class StateMachine<T>
    {
        private T _owner;

        public IState<T> CurrentState { get; private set; }

        public IState<T> PreviousState { get; private set; }

        public bool IsConfigured => CurrentState != null && PreviousState != null;

        public void UpdateLogic()
        {
            CurrentState?.UpdateLogic(_owner);
        }

        public void UpdatePhysics()
        {
            CurrentState?.UpdatePhysics(_owner);
        }

        public void ChangeState(IState<T> newState)
        {
            PreviousState = CurrentState;

            CurrentState?.Exit(_owner);
            CurrentState = newState;
            CurrentState?.Enter(_owner);
        }

        public void Configure(T owner, IState<T> initialState)
        {
            _owner = owner;
            ChangeState(initialState);
        }

        public void RevertToPreviousState()
        {
            if (PreviousState != null)
                ChangeState(PreviousState);
        }
    }
}