using RubyCase.StateMachine;

namespace RubyCase.BoxSystem
{
    public class BoxOnBenchState : IState<BoxController>
    {
        public void OnEnter(BoxController owner)
        {
            owner.SetClickable(true);
            owner.NotifyBenchArrived();
        }

        public void Tick(BoxController owner)   { }
        public void OnExit(BoxController owner) { }
    }
}