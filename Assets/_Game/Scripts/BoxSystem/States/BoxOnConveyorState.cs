using DG.Tweening;
using RubyCase.StateMachine;
using UnityEngine;

namespace RubyCase.BoxSystem
{
    public class BoxOnConveyorState : IState<BoxController>
    {
        private Tween _tween;
        private Vector3[] _waypoints;

        public void SetWaypoints(Vector3[] waypoints)
        {
            _waypoints = waypoints;
        }

        public void OnEnter(BoxController owner)
        {
            owner.SetClickable(false);

            if (_waypoints == null || _waypoints.Length < 2)
            {
                Debug.LogWarning("BoxOnConveyorState: waypoints invalid.");
                owner.StateMachine.TransitionTo(owner.IdleState);
                return;
            }

            _tween = owner.transform
                .DOPath(_waypoints, _waypoints.Length * 0.4f, PathType.Linear)
                .SetEase(Ease.Linear)
                .SetLookAt(0.01f)
                .OnWaypointChange(wi => owner.NotifyNodeReached(wi))
                .OnComplete(() => owner.NotifyPathCompleted());
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