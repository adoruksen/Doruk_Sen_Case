using DG.Tweening;
using RubyCase.LevelSystem;
using RubyCase.StateMachine;
using UnityEngine;

namespace RubyCase.BoxSystem
{
    public class BoxOnConveyorState : IState<BoxController>
    {
        private Tween _tween;
        private ConveyorPathData _path;
        private Vector3 _origin;
        private int[] _nodeMap;

        public void SetPath(ConveyorPathData path, Vector3 origin)
        {
            _path = path;
            _origin = origin;
        }

        public void OnEnter(BoxController owner)
        {
            owner.SetClickable(false);

            if (_path == null || _path.NodeCount < 2)
            {
                Debug.LogWarning("BoxOnConveyorState: path invalid.");
                owner.StateMachine.TransitionTo(owner.IdleState);
                return;
            }

            var waypoints = BuildWaypoints(out _nodeMap);

            _tween = owner.transform
                .DOPath(waypoints, waypoints.Length * 0.5f, PathType.Linear)
                .SetEase(Ease.Linear)
                .SetLookAt(0.01f)
                .OnWaypointChange(wi => owner.NotifyNodeReached(_nodeMap[wi]))
                .OnComplete(() => owner.NotifyPathCompleted());
        }

        public void Tick(BoxController owner)
        {
        }

        public void OnExit(BoxController owner)
        {
            _tween?.Kill();
            _tween = null;
            _path = null;
            _origin = default;
            _nodeMap = null;
        }

        private Vector3[] BuildWaypoints(out int[] nodeMap)
        {
            var pts = new Vector3[_path.NodeCount];
            nodeMap = new int[_path.NodeCount];
            for (int i = 0; i < _path.NodeCount; i++)
            {
                pts[i] = _path.GetWorldPosition(i, _origin);
                nodeMap[i] = i;
            }

            return pts;
        }
    }
}