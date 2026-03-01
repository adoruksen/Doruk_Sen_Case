using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using RubyCase.LevelSystem;
using RubyCase.TeamSystem;
using RubyCase.Core.Session;
using RubyCase.Gameplay.BenchSystem;
using RubyCase.Gameplay.BoxSystem;
using RubyCase.Pool;
using Zenject;

namespace RubyCase.Core.Level
{
    public class LevelInstantiator : ILevelInstantiator, IInitializable, IDisposable
    {
        private readonly ILevelManager _levelManager;
        private readonly AddressableGroupConfig _config;
        private readonly LevelCreationSettings _settings;
        private readonly DiContainer _container;
        private readonly IBoxManager _boxManager;
        private readonly IPoolManager _pool;

        private readonly HashSet<GameObject> _spawnedThisLevel = new();

        private AsyncOperationHandle<GameObject> _conveyorHandle;
        private bool _conveyorHandleValid;

        public LevelInstantiator(
            ILevelManager levelManager,
            AddressableGroupConfig config,
            LevelCreationSettings settings,
            DiContainer container,
            IBoxManager boxManager,
            IPoolManager pool)
        {
            _levelManager = levelManager;
            _config = config;
            _settings = settings;
            _container = container;
            _boxManager = boxManager;
            _pool = pool;
        }

        public void Initialize()
        {
            _levelManager.OnSpawnRequested += OnSpawnRequested;
            _levelManager.OnLevelCleared += ReleaseAll;
        }

        public void Dispose()
        {
            _levelManager.OnSpawnRequested -= OnSpawnRequested;
            _levelManager.OnLevelCleared -= ReleaseAll;
            ReleaseAll();
        }

        private void OnSpawnRequested(LevelData data) => SpawnLevelAsync(data).Forget();

        private async UniTaskVoid SpawnLevelAsync(LevelData data)
        {
            var ctx = _levelManager.CurrentContext;
            if (ctx == null)
            {
                Debug.LogError("[LevelInstantiator] Context is null.");
                return;
            }

            var layout = LevelLayout.Calculate(data, _settings);
            ctx.CollectablesBottomLeft = layout.CollectablesBottomLeft;
            ctx.CollectablesRoot.position = layout.CollectablesCenter;
            ctx.BoxesRoot.position = layout.BoxesCenter;
            ctx.ConveyorRoot.position = layout.ConveyorCenter;
            ctx.BenchesRoot.position = new Vector3(0f, 0f, layout.BenchRowZ);

            await SpawnConveyorAsync(ctx.ConveyorRoot);
            await SpawnCollectablesAsync(data, ctx.CollectablesRoot, layout);
            await SpawnBenchesAsync(data, ctx.BenchesRoot);
            await SpawnBoxesAsync(data, ctx.BoxesRoot, layout);

            _levelManager.NotifySpawnComplete();
        }

        private async UniTask SpawnConveyorAsync(Transform root)
        {
            if (!IsValidRef(_config.ConveyorPrefab))
            {
                Debug.LogWarning("[LevelInstantiator] ConveyorPrefab is null or invalid.");
                return;
            }

            _conveyorHandle = Addressables.InstantiateAsync(_config.ConveyorPrefab, root.position, Quaternion.identity, root);
            _conveyorHandleValid = true;
            await _conveyorHandle;
            if (_conveyorHandle.Status != AsyncOperationStatus.Succeeded)
            {
                Debug.LogError("[LevelInstantiator] Failed to instantiate ConveyorPrefab.");
                _conveyorHandleValid = false;
            }
        }

        private async UniTask SpawnCollectablesAsync(LevelData data, Transform root, LevelLayout.Result layout)
        {
            int w = Mathf.Max(1, data.collectableGridWidth);
            int h = Mathf.Max(1, data.collectableGridHeight);
            var itemScale = Vector3.one * layout.CellSize;

            foreach (var cell in data.collectableCells)
            {
                if (!cell.isFilled || cell.team == null) continue;
                var pos = root.position + LevelLayout.GridLocalOffsetCentered(cell.position, w, h, layout.CellPitch);
                var go = await PoolSpawnAsync(_config.CollectablePrefab, pos, root);
                if (go == null) continue;
                go.transform.localScale = itemScale;
                go.GetComponent<IHaveTeam>()?.AssignTeam(cell.team);
                cell.SpawnedObject = go;
                _levelManager.CurrentContext.RegisterCollectable(go);
            }
        }

        private async UniTask SpawnBoxesAsync(LevelData data, Transform root, LevelLayout.Result layout)
        {
            int bw = Mathf.Max(1, data.boxGridWidth);
            int bh = Mathf.Max(1, data.boxGridHeight);
            _boxManager.SetupGrid(bw, bh, _settings.BoxPitch);

            foreach (var cell in data.boxCells)
            {
                if (!cell.isFilled || cell.team == null) continue;
                var pos = root.position +
                          LevelLayout.GridLocalOffsetCentered(cell.position, bw, bh, _settings.BoxPitch);
                var go = await PoolSpawnAsync(_config.BoxPrefab, pos, root);
                if (go == null) continue;
                go.GetComponent<IHaveTeam>()?.AssignTeam(cell.team);
                _container.InjectGameObject(go);
                if (go.TryGetComponent<BoxController>(out var box))
                {
                    if (box.Capacity == 0)
                        Debug.LogWarning($"[LevelInstantiator] '{go.name}' has no BoxSlot components.");
                    _boxManager.RegisterBox(box, cell.position.x, cell.position.y);
                }

                _levelManager.CurrentContext.RegisterBox(go);
            }
        }

        private async UniTask SpawnBenchesAsync(LevelData data, Transform root)
        {
            if (!IsValidRef(_config.BenchPrefab)) return;
            int count = Mathf.Clamp(data.benchCapacity, 1, 8);
            float spacing = Mathf.Max(0.01f, _settings.BenchSpacingX);
            for (int i = 0; i < count; i++)
            {
                var localPos = new Vector3((i - (count - 1) * 0.5f) * spacing, 0f, 0f);
                var go = await PoolSpawnAsync(_config.BenchPrefab, root.position + localPos, root);
                if (go == null) continue;
                if (!go.TryGetComponent<BenchController>(out _))
                    Debug.LogWarning($"[LevelInstantiator] '{go.name}' has no BenchController.");
                go.transform.localPosition = localPos;
                _levelManager.CurrentContext.RegisterBench(go);
            }
        }

        private async UniTask<GameObject> PoolSpawnAsync(AssetReferenceGameObject assetRef, Vector3 position, Transform parent)
        {
            if (!IsValidRef(assetRef))
            {
                Debug.LogWarning("[LevelInstantiator] Invalid asset ref.");
                return null;
            }

            var go = await _pool.SpawnAsync(assetRef, position, Quaternion.identity, parent);
            if (go != null) _spawnedThisLevel.Add(go);
            return go;
        }

        public void ReleaseAll()
        {
            foreach (var go in _spawnedThisLevel)
                _pool.Release(go);
            _spawnedThisLevel.Clear();

            if (_conveyorHandleValid && _conveyorHandle.IsValid())
            {
                Addressables.ReleaseInstance(_conveyorHandle);
                _conveyorHandleValid = false;
            }
        }

        private static bool IsValidRef(AssetReferenceGameObject referance) =>
            referance != null && referance.RuntimeKeyIsValid();
    }
}