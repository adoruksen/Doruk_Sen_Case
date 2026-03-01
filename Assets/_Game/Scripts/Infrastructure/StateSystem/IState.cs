namespace RubyCase.StateMachine
{
    public interface IState<TOwner>
    {
        void OnEnter(TOwner owner);
        void Tick(TOwner owner);
        void OnExit(TOwner owner);
    }
}