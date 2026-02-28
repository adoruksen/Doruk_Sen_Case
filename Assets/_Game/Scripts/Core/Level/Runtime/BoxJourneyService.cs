using System;
using System.Collections.Generic;
using DG.Tweening;
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
        private readonly LevelCreationSettings _settings;
        private readonly IBoxManager _boxManager;

        private readonly Dictionary<BoxController, Handlers> _active = new();

        private class Handlers
        {
            public Action<int> OnNode;
            public Action OnComplete;
            public Action OnFullyLoaded;
            public Action OnDestroy;
        }

        public BoxJourneyService(IConveyorManager conveyor, IBenchManager bench, ILevelManager levelManager, LevelCreationSettings settings, IBoxManager boxManager)
        {
            _conveyor = conveyor;
            _bench = bench;
            _levelManager = levelManager;
            _settings = settings;
            _boxManager = boxManager;
        }

        public bool CanStartJourney(BoxController box) =>
            box.StateMachine.CurrentState == box.IdleState ||
            box.StateMachine.CurrentState == box.OnBenchState;

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
                OnFullyLoaded = () => OnBoxFullyLoaded(box),
                OnDestroy = () => Detach(box),
            };

            _active[box] = h;
            box.OnNodeReached += h.OnNode;
            box.OnPathCompleted += h.OnComplete;
            box.OnFullyLoaded += h.OnFullyLoaded;
            box.OnDestroyed += h.OnDestroy;

            if (box.StateMachine.CurrentState == box.OnBenchState)
                _bench.Release(box);

            _boxManager.OnBoxDeparted(box);

            box.MoveToConveyorState.SetWaypoints(_conveyor.Waypoints, _conveyor.RoadStartIndex);
            box.OnConveyorState.SetWaypoints(_conveyor.Waypoints, _settings.ConveyorSpeed);
            box.StateMachine.TransitionTo(box.MoveToConveyorState);
        }

        private void OnNodeReached(BoxController box, int waypointIndex)
        {
            if (box.IsFull) return;

            var session = _levelManager.CurrentSession;
            if (session == null) return;

            var scan = _conveyor.GetScanInfo(waypointIndex);
            if (scan == null) return;

            var level = _levelManager.CurrentData;
            var cell = FindNearestCell(scan.Value, level);

            if (cell == null || cell.team != box.Team) return;

            var slot = box.GetAvailableSlot();
            if (slot == null) return;

            var go = cell.SpawnedObject;
            cell.SpawnedObject = null;

            box.Collect(slot, go);
            session.Collectables.MarkResolved(go);

            go.transform.SetParent(slot.transform, true);
            go.transform.DOLocalMove(Vector3.zero, 0.35f)
                .SetEase(Ease.OutQuad)
                .OnComplete(() =>
                {
                    slot.Occupy();
                    box.NotifySlotOccupied();
                });
        }

        private void OnBoxFullyLoaded(BoxController box)
        {
            Detach(box);
            _levelManager.CurrentSession?.Boxes.MarkResolved(box.gameObject);
            box.StateMachine.TransitionTo(box.IdleState);
            box.gameObject.SetActive(false);
        }

        private void OnPathCompleted(BoxController box)
        {
            Detach(box);
            if (!box.gameObject.activeSelf) return;
            if (!_bench.TryPlace(box))
                _levelManager.CurrentSession?.Fail();
        }

        private static CollectableGridCellData FindNearestCell(ScanInfo scan, LevelData level)
        {
            int n = level.collectableGridWidth;

            if (scan.IsColumn)
            {
                int col = scan.LineIndex;
                if (col < 0 || col >= n) return null;
                int maxDepth = Mathf.Max(1, n / 2);
                if (scan.FromNear)
                    for (int y = 0; y < maxDepth; y++)
                    {
                        var c = level.GetCollectableCell(col, y);
                        if (IsCollectable(c)) return c;
                    }
                else
                    for (int y = n - 1; y >= n - maxDepth; y--)
                    {
                        var c = level.GetCollectableCell(col, y);
                        if (IsCollectable(c)) return c;
                    }
            }
            else
            {
                int row = scan.LineIndex;
                if (row < 0 || row >= n) return null;
                int maxDepth = Mathf.Max(1, n / 2);
                if (scan.FromNear)
                    for (int x = 0; x < maxDepth; x++)
                    {
                        var c = level.GetCollectableCell(x, row);
                        if (IsCollectable(c)) return c;
                    }
                else
                    for (int x = n - 1; x >= n - maxDepth; x--)
                    {
                        var c = level.GetCollectableCell(x, row);
                        if (IsCollectable(c)) return c;
                    }
            }

            return null;
        }

        private static bool IsCollectable(CollectableGridCellData cell) =>
            cell != null && cell.isFilled && cell.SpawnedObject != null;

        private void Detach(BoxController box)
        {
            if (!_active.TryGetValue(box, out var h)) return;
            box.OnNodeReached -= h.OnNode;
            box.OnPathCompleted -= h.OnComplete;
            box.OnFullyLoaded -= h.OnFullyLoaded;
            box.OnDestroyed -= h.OnDestroy;
            _active.Remove(box);
        }
    }
}