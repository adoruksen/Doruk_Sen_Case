using System;
using RubyCase.Core;
using RubyCase.LevelSystem;
using UnityEngine;
using Zenject;

namespace RubyCase.BoxSystem
{
    public class ConveyorManager : IConveyorManager, IInitializable, IDisposable
    {
        public ConveyorPathData Path { get; private set; }
        public Vector3 GridWorldOrigin { get; private set; }
        public bool IsReady => Path != null;

        private readonly ILevelManager _levelManager;

        public ConveyorManager(ILevelManager levelManager)
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
            Path = _levelManager.CurrentData?.conveyorPath;
            GridWorldOrigin = _levelManager.CurrentContext?.CollectablesBottomLeft ?? Vector3.zero;
        }

        private void OnLevelCleared()
        {
            Path = null;
            GridWorldOrigin = default;
        }
    }
}