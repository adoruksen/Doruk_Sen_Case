using RubyCase.StateMachine;

namespace RubyCase.Gameplay.BoxSystem
{
    public class BoxIdleState : IState<BoxController>
    {
        public void OnEnter(BoxController owner) => owner.SetClickable(true);
        public void Tick(BoxController owner)    { }
        public void OnExit(BoxController owner)  { }
    }
}