using DG.Tweening;
using RubyCase.Core;
using RubyCase.StateMachine;
using UnityEngine;

namespace RubyCase.BoxSystem
{
    public class BoxOnConveyorState : IState<BoxController>
    {
        private Tween _tween;
        private Vector3[] _waypoints;
        private float _speed;

        public void SetWaypoints(Vector3[] waypoints, float speed)
        {
            _waypoints = waypoints;
            _speed = speed;
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

            float totalLength = 0f;
            for (int i = 1; i < _waypoints.Length; i++)
                totalLength += Vector3.Distance(_waypoints[i - 1], _waypoints[i]);

            float duration = totalLength / Mathf.Max(0.01f, _speed);

            _tween = owner.transform
                .DOPath(_waypoints, duration, PathType.Linear)
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