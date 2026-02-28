using System;
using System.Collections.Generic;
using RubyCase.Core;
using RubyCase.LevelSystem;
using UnityEngine;

namespace RubyCase.BoxSystem
{
    public class BoxJourneyService : IBoxJourneyService
    {
        private readonly IConveyorManager _conveyor;
        private readonly IBenchManager _bench;
        private readonly ILevelManager _levelManager;

        private readonly Dictionary<BoxController, (Action<int> onNode, Action onComplete, Action onDestroy)> _active =
            new();

        public BoxJourneyService(IConveyorManager conveyor, IBenchManager bench, ILevelManager levelManager)
        {
            _conveyor = conveyor;
            _bench = bench;
            _levelManager = levelManager;
        }

        public bool CanStartJourney(BoxController box)
        {
            return box.StateMachine.CurrentState == box.IdleState || box.StateMachine.CurrentState == box.OnBenchState;
        }

        public void StartJourney(BoxController box)
        {
            if (!CanStartJourney(box)) return;
            if (!_conveyor.IsReady)
            {
                Debug.LogWarning("BoxJourneyService: conveyor not ready.");
                return;
            }

            Detach(box);

            Action<int> onNode = i => OnNodeReached(box, i);
            Action onComplete = () => OnPathCompleted(box);
            Action onDestroy = () => Detach(box);

            _active[box] = (onNode, onComplete, onDestroy);
            box.OnNodeReached += onNode;
            box.OnPathCompleted += onComplete;
            box.OnDestroyed += onDestroy;

            if (box.StateMachine.CurrentState == box.OnBenchState) _bench.Release(box);

            box.MoveToConveyorState.SetPath(_conveyor.Path, _conveyor.GridWorldOrigin);
            box.OnConveyorState.SetPath(_conveyor.Path, _conveyor.GridWorldOrigin);
            box.StateMachine.TransitionTo(box.MoveToConveyorState);
        }

        private void OnNodeReached(BoxController box, int nodeIndex)
        {
            var session = _levelManager.CurrentSession;
            if (session == null) return;

            var node = _conveyor.Path?.GetNode(nodeIndex);
            if (node == null || node.isCorner) return;

            foreach (var cell in ConveyorScanMapper.GetAlignedCells(node, _levelManager.CurrentData))
            {
                if (box.IsFull) break;
                if (cell.team != box.Team) continue;

                var slot = box.GetAvailableSlot();
                if (slot == null) break;

                box.Collect(slot, cell.SpawnedObject);

                if (cell.SpawnedObject != null)
                {
                    session.Collectables.MarkResolved(cell.SpawnedObject);
                    UnityEngine.Object.Destroy(cell.SpawnedObject);
                }

                cell.Clear();
            }
        }

        private void OnPathCompleted(BoxController box)
        {
            Detach(box);

            var session = _levelManager.CurrentSession;

            if (box.IsFull)
            {
                session?.Boxes.MarkResolved(box.gameObject);
                UnityEngine.Object.Destroy(box.gameObject);
                return;
            }

            if (!_bench.TryPlace(box))
                session?.Fail();
        }

        private void Detach(BoxController box)
        {
            if (!_active.TryGetValue(box, out var handles)) return;
            box.OnNodeReached -= handles.onNode;
            box.OnPathCompleted -= handles.onComplete;
            box.OnDestroyed -= handles.onDestroy;
            _active.Remove(box);
        }
    }
}