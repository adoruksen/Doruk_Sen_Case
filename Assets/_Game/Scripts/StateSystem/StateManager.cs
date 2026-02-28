namespace RubyCase.StateMachine
{
    public class StateManager<TOwner> where TOwner : class
    {
        public IState<TOwner> CurrentState { get; private set; }

        private readonly TOwner _owner;

        public StateManager(TOwner owner) => _owner = owner;

        public void TransitionTo(IState<TOwner> next)
        {
            if (next == null || next == CurrentState) return;
            CurrentState?.OnExit(_owner);
            CurrentState = next;
            CurrentState.OnEnter(_owner);
        }

        public void Tick() => CurrentState?.Tick(_owner);
    }
}