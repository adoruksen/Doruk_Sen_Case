using DG.Tweening;
using RubyCase.StateMachine;
using UnityEngine;

namespace RubyCase.BoxSystem
{
    public class BoxMoveToBenchState : IState<BoxController>
    {
        private Tween _tween;
        private Transform _slot;

        public void MoveTo(Transform slot) => _slot = slot;

        public void OnEnter(BoxController owner)
        {
            owner.SetClickable(false);

            if (_slot == null)
            {
                Debug.LogWarning("BoxMoveToBenchState: no slot assigned.");
                owner.StateMachine.TransitionTo(owner.IdleState);
                return;
            }

            _tween = owner.transform
                .DOMove(_slot.position, 0.4f)
                .SetEase(Ease.InOutQuad)
                .OnComplete(() => owner.StateMachine.TransitionTo(owner.OnBenchState));
        }

        public void Tick(BoxController owner)
        {
        }

        public void OnExit(BoxController owner)
        {
            _tween?.Kill();
            _tween = null;
            _slot = null;
        }
    }
}