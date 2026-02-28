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

        private readonly Dictionary<BoxController, Handlers> _active = new();

        private class Handlers
        {
            public Action<int> OnNode;
            public Action OnComplete;
            public Action OnDestroy;
        }

        public BoxJourneyService(IConveyorManager conveyor, IBenchManager bench, ILevelManager levelManager)
        {
            _conveyor = conveyor;
            _bench = bench;
            _levelManager = levelManager;
        }

        public bool CanStartJourney(BoxController box)
        {
            return box.StateMachine.CurrentState == box.IdleState
                   || box.StateMachine.CurrentState == box.OnBenchState;
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

            var h = new Handlers
            {
                OnNode = i => OnNodeReached(box, i),
                OnComplete = () => OnPathCompleted(box),
                OnDestroy = () => Detach(box),
            };

            _active[box] = h;
            box.OnNodeReached += h.OnNode;
            box.OnPathCompleted += h.OnComplete;
            box.OnDestroyed += h.OnDestroy;

            if (box.StateMachine.CurrentState == box.OnBenchState)
                _bench.Release(box);

            box.MoveToConveyorState.SetPath(_conveyor.Path, _conveyor.GridWorldOrigin);
            box.OnConveyorState.SetPath(_conveyor.Path, _conveyor.GridWorldOrigin);
            box.StateMachine.TransitionTo(box.MoveToConveyorState);
        }

        private void OnNodeReached(BoxController box, int nodeIndex)
        {
            if (box.IsFull) return;

            var session = _levelManager.CurrentSession;
            if (session == null) return;

            var node = _conveyor.Path?.GetNode(nodeIndex);
            if (node == null || node.scanAxis == ScanAxis.None) return;

            var cell = ConveyorScanMapper.GetNearestCell(node, _levelManager.CurrentData);
            if (cell == null || cell.team != box.Team) return;

            var slot = box.GetAvailableSlot();
            if (slot == null) return;

            box.Collect(slot, cell.SpawnedObject);
            session.Collectables.MarkResolved(cell.SpawnedObject);
            cell.SpawnedObject.SetActive(false);
            cell.SpawnedObject = null;
        }

        private void OnPathCompleted(BoxController box)
        {
            Detach(box);

            var session = _levelManager.CurrentSession;

            if (box.IsFull)
            {
                session?.Boxes.MarkResolved(box.gameObject);
                box.gameObject.SetActive(false);
                return;
            }

            if (!_bench.TryPlace(box))
                session?.Fail();
        }

        private void Detach(BoxController box)
        {
            if (!_active.TryGetValue(box, out var h)) return;
            box.OnNodeReached -= h.OnNode;
            box.OnPathCompleted -= h.OnComplete;
            box.OnDestroyed -= h.OnDestroy;
            _active.Remove(box);
        }
    }
}