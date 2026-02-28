using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using RubyCase.LevelSystem;
using RubyCase.TeamSystem;
using RubyCase.BoxSystem;
using Zenject;

namespace RubyCase.Core
{
    public class LevelInstantiator : ILevelInstantiator, IInitializable, IDisposable
    {
        private readonly ILevelManager _levelManager;
        private readonly AddressableGroupConfig _config;
        private readonly LevelCreationSettings _settings;
        private readonly DiContainer _container;

        private readonly List<AsyncOperationHandle> _handles = new();

        public LevelInstantiator(ILevelManager levelManager, AddressableGroupConfig config, LevelCreationSettings settings, DiContainer container)
        {
            _levelManager = levelManager;
            _config = config;
            _settings = settings;
            _container = container;
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
                Debug.LogError("LevelInstantiator: context is null.");
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
            var go = await SpawnAsync(_config.ConveyorPrefab, root.position, root);
            if (go == null)
                Debug.LogWarning("LevelInstantiator: ConveyorPrefab missing in AddressableGroupConfig.");
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
                var go = await SpawnAsync(_config.CollectablePrefab, pos, root);
                if (go == null) continue;

                go.transform.localScale = itemScale;
                go.GetComponent<IHaveTeam>()?.AssignTeam(cell.team);
                cell.SpawnedObject = go;
                _levelManager.CurrentContext.RegisterCollectable(go);
            }
        }

        private async UniTask SpawnBoxesAsync(LevelData data, Transform root, LevelLayout.Result layout)
        {
            int w = Mathf.Max(1, data.boxGridWidth);
            int h = Mathf.Max(1, data.boxGridHeight);

            foreach (var cell in data.boxCells)
            {
                if (!cell.isFilled || cell.team == null) continue;

                var pos = root.position + LevelLayout.GridLocalOffsetCentered(cell.position, w, h, _settings.BoxPitch);
                var go = await SpawnAsync(_config.BoxPrefab, pos, root);
                if (go == null) continue;

                go.GetComponent<IHaveTeam>()?.AssignTeam(cell.team);
                _container.InjectGameObject(go);

                if (go.TryGetComponent<BoxController>(out var box) && box.Capacity == 0)
                    Debug.LogWarning($"LevelInstantiator: '{go.name}' has no BoxSlot components.");

                _levelManager.CurrentContext.RegisterBox(go);
            }
        }

        private async UniTask SpawnBenchesAsync(LevelData data, Transform root)
        {
            if (_config.BenchPrefab == null || !_config.BenchPrefab.RuntimeKeyIsValid()) return;

            int count = Mathf.Clamp(data.benchCapacity, 1, 8);
            float spacing = Mathf.Max(0.01f, _settings.BenchSpacingX);

            for (int i = 0; i < count; i++)
            {
                var localPos = new Vector3((i - (count - 1) * 0.5f) * spacing, 0f, 0f);
                var go = await SpawnAsync(_config.BenchPrefab, root.position + localPos, root);
                if (go == null) continue;

                if (!go.TryGetComponent<BenchController>(out _))
                    Debug.LogWarning($"LevelInstantiator: BenchPrefab '{go.name}' has no BenchController.");

                go.transform.localPosition = localPos;
                _levelManager.CurrentContext.RegisterBench(go);
            }
        }

        private async UniTask<GameObject> SpawnAsync(AssetReferenceGameObject assetRef,
            Vector3 position, Transform parent)
        {
            if (assetRef == null || !assetRef.RuntimeKeyIsValid())
            {
                Debug.LogWarning("LevelInstantiator: skipping null/invalid asset reference.");
                return null;
            }

            var handle = Addressables.InstantiateAsync(assetRef, position, Quaternion.identity, parent);
            _handles.Add(handle);
            await handle.ToUniTask();

            if (handle.Status != AsyncOperationStatus.Succeeded)
            {
                Debug.LogError($"LevelInstantiator: failed to spawn {assetRef.RuntimeKey}");
                return null;
            }

            return handle.Result;
        }

        public void ReleaseAll()
        {
            foreach (var h in _handles)
                if (h.IsValid())
                    Addressables.ReleaseInstance(h);
            _handles.Clear();
        }
    }
}