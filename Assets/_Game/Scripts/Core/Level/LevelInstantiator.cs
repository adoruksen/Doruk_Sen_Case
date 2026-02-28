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

        private void OnSpawnRequested(LevelData data) => SpawnAsync(data).Forget();

        private async UniTaskVoid SpawnAsync(LevelData data)
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
            ctx.ConveyorNodesRoot.position = layout.CollectablesCenter;
            ctx.BenchesRoot.position = new Vector3(0f, 0f, layout.BenchRowZ);

            await SpawnCollectablesAsync(data, ctx.CollectablesRoot);
            await SpawnConveyorAsync(data, ctx.ConveyorNodesRoot, layout);
            await SpawnBenchesAsync(data, ctx.BenchesRoot);
            await SpawnBoxesAsync(data, ctx.BoxesRoot);

            _levelManager.NotifySpawnComplete();
        }

        private async UniTask SpawnCollectablesAsync(LevelData data, Transform root)
        {
            int w = Mathf.Max(1, data.collectableGridWidth);
            int h = Mathf.Max(1, data.collectableGridHeight);

            foreach (var cell in data.collectableCells)
            {
                if (!cell.isFilled || cell.team == null) continue;

                var pos = root.position + LevelLayout.GridLocalOffsetCentered(cell.position, w, h, _settings.CellSize);
                var go = await SpawnAsync(_config.CollectablePrefab, pos, root);
                if (go == null) continue;

                go.GetComponent<IHaveTeam>()?.AssignTeam(cell.team);
                cell.SpawnedObject = go;
                _levelManager.CurrentContext.RegisterCollectable(go);
            }
        }

        private async UniTask SpawnBoxesAsync(LevelData data, Transform root)
        {
            int w = Mathf.Max(1, data.boxGridWidth);
            int h = Mathf.Max(1, data.boxGridHeight);

            foreach (var cell in data.boxCells)
            {
                if (!cell.isFilled || cell.team == null) continue;

                var pos = root.position + LevelLayout.GridLocalOffsetCentered(cell.position, w, h, _settings.CellSize);
                var go = await SpawnAsync(_config.BoxPrefab, pos, root);
                if (go == null) continue;

                go.GetComponent<IHaveTeam>()?.AssignTeam(cell.team);
                _container.InjectGameObject(go);

                if (go.TryGetComponent<BoxController>(out var box) && box.Capacity == 0)
                    Debug.LogWarning(
                        $"LevelInstantiator: '{go.name}' has no BoxSlot components. Add 4 BoxSlot children to the prefab.");

                _levelManager.CurrentContext.RegisterBox(go);
            }
        }

        private async UniTask SpawnConveyorAsync(LevelData data, Transform root, LevelLayout.Result layout)
        {
            var path = data.conveyorPath;
            if (path == null || path.gridWidth != data.collectableGridWidth ||
                path.gridHeight != data.collectableGridHeight)
                path = ConveyorScanMapper.GeneratePath(data.collectableGridWidth, data.collectableGridHeight);

            path.cellSize = _settings.CellSize;

            for (int i = 0; i < path.NodeCount; i++)
            {
                var node = path.GetNode(i);
                if (node == null) continue;

                var worldPos = path.GetWorldPosition(i, layout.CollectablesBottomLeft);
                var go = await SpawnAsync(_config.ConveyorNodePrefab, worldPos, root);
                if (go == null) continue;

                go.transform.localPosition = worldPos - root.position;
                _levelManager.CurrentContext.RegisterConveyorNode(go);
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
                    Debug.LogWarning($"LevelInstantiator: BenchPrefab '{go.name}' has no BenchController component.");

                go.transform.localPosition = localPos;
                _levelManager.CurrentContext.RegisterBench(go);
            }
        }

        private async UniTask<GameObject> SpawnAsync(AssetReferenceGameObject assetRef, Vector3 position, Transform parent)
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