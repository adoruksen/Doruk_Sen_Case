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

        private readonly List<AsyncOperationHandle> _handles = new();

        public LevelInstantiator(ILevelManager levelManager, AddressableGroupConfig config, LevelCreationSettings settings)
        {
            _levelManager = levelManager;
            _config = config;
            _settings = settings;
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
            LevelContext ctx = _levelManager.CurrentContext;
            if (ctx == null)
            {
                Debug.LogError("[LevelInstantiator] Missing LevelContext.");
                return;
            }

            var layout = LevelLayout.Calculate(data, _settings);

            ctx.CollectablesRoot.position = layout.CollectablesCenter;
            ctx.BoxesRoot.position = layout.BoxesCenter;
            ctx.ConveyorNodesRoot.position = layout.CollectablesCenter;
            ctx.BenchesRoot.position = new Vector3(0f, 0f, layout.BenchRowZ);

            await SpawnCollectablesAsync(data, ctx.CollectablesRoot, layout);
            await SpawnConveyorAsync(data, ctx.ConveyorNodesRoot, layout);
            await SpawnBenchesAsync(data, ctx.BenchesRoot, layout);
            await SpawnBoxesAsync(data, ctx.BoxesRoot, layout);
            _levelManager.NotifySpawnComplete();
        }

        private async UniTask SpawnCollectablesAsync(LevelData data, Transform root, LevelLayout.Result layout)
        {
            int w = Mathf.Max(1, data.collectableGridWidth);
            int h = Mathf.Max(1, data.collectableGridHeight);

            foreach (var cell in data.collectableCells)
            {
                if (!cell.isFilled || cell.team == null) continue;

                Vector3 worldPos = root.position +
                                   LevelLayout.GridLocalOffsetCentered(cell.position, w, h, _settings.CellSize);

                GameObject go = await SpawnAsync(
                    _config.CollectablePrefab,
                    worldPos,
                    root);

                if (go == null) continue;
                ApplyTeam(go, cell.team);
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

                Vector3 worldPos = root.position +
                                   LevelLayout.GridLocalOffsetCentered(cell.position, w, h, _settings.CellSize);

                GameObject go = await SpawnAsync(
                    _config.BoxPrefab,
                    worldPos,
                    root);

                if (go == null) continue;
                ApplyTeam(go, cell.team);
                _levelManager.CurrentContext.RegisterBox(go);
            }
        }

        private async UniTask SpawnConveyorAsync(LevelData data, Transform root, LevelLayout.Result layout)
        {
            var path = data.conveyorPath;
            if (path == null || path.gridWidth != data.collectableGridWidth ||
                path.gridHeight != data.collectableGridHeight)
            {
                path = RubyCase.LevelSystem.ConveyorScanMapper.GeneratePath(data.collectableGridWidth,
                    data.collectableGridHeight);
            }

            path.cellSize = _settings.CellSize;

            for (int i = 0; i < path.NodeCount; i++)
            {
                var node = path.GetNode(i);
                if (node == null) continue;

                Vector3 worldPos = path.GetWorldPosition(i, layout.CollectablesBottomLeft);
                Vector3 localPos = worldPos - root.position;

                GameObject go = await SpawnAsync(
                    _config.ConveyorNodePrefab,
                    worldPos,
                    root);

                if (go == null) continue;
                go.transform.localPosition = localPos;
                _levelManager.CurrentContext.RegisterConveyorNode(go);
            }
        }

        private async UniTask SpawnBenchesAsync(LevelData data, Transform root, LevelLayout.Result layout)
        {
            if (_config.BenchPrefab == null || !_config.BenchPrefab.RuntimeKeyIsValid())
                return;

            float spacing = Mathf.Max(0.01f, _settings.BenchSpacingX);
            int count = Mathf.Max(1, data.benchCapacity);
            if (count != 4) count = 4;

            for (int i = 0; i < count; i++)
            {
                float x = (i - (count - 1) * 0.5f) * spacing;
                var localPos = new Vector3(x, 0f, 0f);
                GameObject go = await SpawnAsync(_config.BenchPrefab, root.position + localPos, root);
                if (go == null) continue;
                go.transform.localPosition = localPos;
                _levelManager.CurrentContext.RegisterBench(go);
            }
        }

        private async UniTask<GameObject> SpawnAsync(AssetReferenceGameObject assetRef, Vector3 position, Transform parent)
        {
            if (assetRef == null || !assetRef.RuntimeKeyIsValid())
            {
                Debug.LogWarning("[LevelInstantiator] Invalid AssetReference.");
                return null;
            }

            var handle = Addressables.InstantiateAsync(assetRef, position, Quaternion.identity, parent);
            _handles.Add(handle);

            await handle.ToUniTask();

            if (handle.Status != AsyncOperationStatus.Succeeded)
            {
                Debug.LogError($"[LevelInstantiator] Spawn failed: {assetRef.RuntimeKey}");
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

        private static void ApplyTeam(GameObject go, Team team)
        {
            var item = go.GetComponent<IHaveTeam>();
            item?.AssignTeam(team);
        }
    }
}
