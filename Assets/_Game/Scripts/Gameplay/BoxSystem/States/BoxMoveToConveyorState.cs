using DG.Tweening;
using RubyCase.StateMachine;
using UnityEngine;

namespace RubyCase.Gameplay.BoxSystem
{
    public class BoxMoveToConveyorState : IState<BoxController>
    {
        private Tween _tween;
        private Vector3[] _waypoints;
        private int _startIndex;

        public void SetWaypoints(Vector3[] waypoints, int startIndex)
        {
            _waypoints = waypoints;
            _startIndex = startIndex;
        }

        public void OnEnter(BoxController owner)
        {
            owner.SetClickable(false);

            if (_waypoints == null || _startIndex < 0 || _startIndex >= _waypoints.Length)
            {
                Debug.LogWarning("BoxMoveToConveyorState: waypoints not set.");
                owner.StateMachine.TransitionTo(owner.IdleState);
                return;
            }

            _tween = owner.transform
                .DOMove(_waypoints[_startIndex], 0.3f)
                .SetEase(Ease.InOutQuad)
                .OnComplete(() => owner.StateMachine.TransitionTo(owner.OnConveyorState));
        }

        public void Tick(BoxController owner)
        {
        }

        public void OnExit(BoxController owner)
        {
            _tween?.Kill();
            _tween = null;
            _waypoints = null;
        }
    }
}