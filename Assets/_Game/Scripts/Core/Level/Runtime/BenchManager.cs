using System;
using System.Collections.Generic;
using RubyCase.Core;
using Zenject;

namespace RubyCase.BoxSystem
{
    public class BenchManager : IBenchManager, IInitializable, IDisposable
    {
        private readonly List<BenchController> _benches = new();
        private readonly ILevelManager _levelManager;

        public bool IsFull => GetAvailable() == null;

        public BenchManager(ILevelManager levelManager)
        {
            _levelManager = levelManager;
        }

        public void Initialize()
        {
            _levelManager.OnLevelReady += OnLevelReady;
            _levelManager.OnLevelCleared += OnLevelCleared;
        }

        public void Dispose()
        {
            _levelManager.OnLevelReady -= OnLevelReady;
            _levelManager.OnLevelCleared -= OnLevelCleared;
        }

        private void OnLevelReady()
        {
            _benches.Clear();

            var gos = _levelManager.CurrentContext?.Benches;
            if (gos == null) return;

            foreach (var go in gos)
            {
                var bench = go.GetComponent<BenchController>();
                if (bench != null) _benches.Add(bench);
            }
        }

        private void OnLevelCleared() => _benches.Clear();

        public bool TryPlace(BoxController box)
        {
            var bench = GetAvailable();
            if (bench == null) return false;

            bench.Reserve(box);
            //box.MoveToBenchState.MoveTo(bench.transform);
            //box.StateMachine.TransitionTo(box.MoveToBenchState);
            return true;
        }

        public void NotifyArrived(BoxController box)
        {
            FindBench(box)?.Occupy();
        }

        public void Release(BoxController box)
        {
            FindBench(box)?.Release();
        }

        private BenchController GetAvailable()
        {
            foreach (var b in _benches)
                if (b.IsAvailable)
                    return b;
            return null;
        }

        private BenchController FindBench(BoxController box)
        {
            foreach (var b in _benches)
                if (b.CurrentBox == box)
                    return b;
            return null;
        }
    }
}