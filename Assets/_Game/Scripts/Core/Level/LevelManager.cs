using System;
using Cysharp.Threading.Tasks;
using UnityEngine;
using RubyCase.LevelSystem;
using RubyCase.Core.Session;

namespace RubyCase.Core.Level
{
    public class LevelManager : ILevelManager
    {
        public LevelData CurrentData { get; private set; }
        public LevelContext CurrentContext { get; private set; }
        public ILevelSession CurrentSession { get; private set; }
        public int CurrentIndex { get; private set; } = -1;

        public event Action OnLevelReady;
        public event Action OnLevelCleared;
        public event Action<LevelData> OnSpawnRequested;

        private readonly LevelDatabase _database;

        private GameObject _levelRoot;

        public LevelManager(LevelDatabase database)
        {
            _database = database;
        }

        public async UniTask LoadLevelAsync(int index)
        {
            await ClearAsync();

            LevelData data = _database.GetLevel(index);
            if (data == null)
            {
                Debug.LogError($"[LevelManager] No level at index {index}.");
                return;
            }

            CurrentIndex = index;
            CurrentData = data;

            _levelRoot = new GameObject($"Level_{data.levelID}");
            CurrentContext = _levelRoot.AddComponent<LevelContext>();
            CurrentContext.Initialize(data);

            OnSpawnRequested?.Invoke(data);
        }

        public async UniTask ReloadCurrentAsync() => await LoadLevelAsync(CurrentIndex >= 0 ? CurrentIndex : 0);

        public async UniTask LoadNextAsync()
        {
            int next = CurrentIndex + 1;
            if (next >= _database.TotalLevels) next = 0;
            await LoadLevelAsync(next);
        }

        public void NotifySpawnComplete()
        {
            CurrentContext.MarkReady();
            OnLevelReady?.Invoke();
        }

        public void SetCurrentSession(ILevelSession session)
        {
            CurrentSession = session;
        }

        private async UniTask ClearAsync()
        {
            CurrentContext?.Clear();
            CurrentContext = null;
            CurrentSession = null;

            if (_levelRoot != null)
            {
                GameObject.Destroy(_levelRoot);
                _levelRoot = null;
            }

            CurrentData = null;
            OnLevelCleared?.Invoke();

            await UniTask.Yield();
        }
    }
}
