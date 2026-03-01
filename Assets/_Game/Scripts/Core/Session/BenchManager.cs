using System;
using System.Collections.Generic;
using RubyCase.Core.Level;
using RubyCase.Gameplay.BenchSystem;
using RubyCase.Gameplay.BoxSystem;
using Zenject;

namespace RubyCase.Core.Session
{
    public sealed class BenchManager : IBenchManager, IInitializable, IDisposable
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
            box.OnBenchArrived += OnBenchArrived;
            box.MoveToBenchState.MoveTo(bench.transform);
            box.StateMachine.TransitionTo(box.MoveToBenchState);
            return true;

            void OnBenchArrived()
            {
                box.OnBenchArrived -= OnBenchArrived;
                NotifyArrived(box);
            }
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
            foreach (var bench in _benches)
                if (bench.IsAvailable)
                    return bench;
            return null;
        }

        private BenchController FindBench(BoxController box)
        {
            foreach (var bench in _benches)
                if (bench.CurrentBox == box)
                    return bench;
            return null;
        }
    }
}