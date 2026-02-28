using RubyCase.StateMachine;

namespace RubyCase.BoxSystem
{
    public class BoxIdleState : IState<BoxController>
    {
        public void OnEnter(BoxController owner) => owner.SetClickable(true);
        public void Tick(BoxController owner)    { }
        public void OnExit(BoxController owner)  { }
    }
}