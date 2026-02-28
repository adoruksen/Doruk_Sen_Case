using DG.Tweening;
using RubyCase.LevelSystem;
using RubyCase.StateMachine;
using UnityEngine;

namespace RubyCase.BoxSystem
{
    public class BoxMoveToConveyorState : IState<BoxController>
    {
        private Tween _tween;
        private ConveyorPathData _path;
        private Vector3 _origin;

        public void SetPath(ConveyorPathData path, Vector3 origin)
        {
            _path = path;
            _origin = origin;
        }

        public void OnEnter(BoxController owner)
        {
            owner.SetClickable(false);

            if (_path == null || _path.NodeCount == 0)
            {
                Debug.LogWarning("BoxMoveToConveyorState: no path assigned.");
                owner.StateMachine.TransitionTo(owner.IdleState);
                return;
            }

            var target = _path.GetWorldPosition(_path.roadStartIndex, _origin);

            _tween = owner.transform
                .DOMove(target, 0.3f)
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
            _path = null;
            _origin = default;
        }
    }
}