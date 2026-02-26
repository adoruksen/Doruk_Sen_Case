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

        private readonly List<AsyncOperationHandle> _handles = new();

        public LevelInstantiator(ILevelManager levelManager, AddressableGroupConfig config)
        {
            _levelManager = levelManager;
            _config = config;
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

            await SpawnCollectablesAsync(data, ctx.CollectablesRoot);
            await SpawnBoxesAsync(data, ctx.BoxesRoot);
            await SpawnConveyorAsync(data, ctx.ConveyorNodesRoot);

            _levelManager.NotifySpawnComplete();
        }

        private async UniTask SpawnCollectablesAsync(LevelData data, Transform root)
        {
            foreach (var cell in data.collectableCells)
            {
                if (!cell.isFilled || cell.team == null) continue;

                GameObject go = await SpawnAsync(
                    _config.CollectablePrefab,
                    GridToWorld(cell.position, _config.CellSize),
                    root);

                if (go == null) continue;
                ApplyTeam(go, cell.team);
                _levelManager.CurrentContext.RegisterCollectable(go);
            }
        }

        private async UniTask SpawnBoxesAsync(LevelData data, Transform root)
        {
            foreach (var cell in data.boxCells)
            {
                if (!cell.isFilled || cell.team == null) continue;

                GameObject go = await SpawnAsync(
                    _config.BoxPrefab,
                    BoxGridToWorld(cell.position, _config),
                    root);

                if (go == null) continue;
                ApplyTeam(go, cell.team);
                _levelManager.CurrentContext.RegisterBox(go);
            }
        }

        private async UniTask SpawnConveyorAsync(LevelData data, Transform root)
        {
            if (data.conveyorPath == null) return;

            for (int i = 0; i < data.conveyorPath.NodeCount; i++)
            {
                var node = data.conveyorPath.GetNode(i);
                if (node == null) continue;

                GameObject go = await SpawnAsync(
                    _config.ConveyorNodePrefab,
                    ConveyorToWorld(node.localPosition, _config.CellSize),
                    root);

                if (go == null) continue;
                _levelManager.CurrentContext.RegisterConveyorNode(go);
            }
        }

        private async UniTask<GameObject> SpawnAsync(
            AssetReferenceGameObject assetRef,
            Vector3 position,
            Transform parent)
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
                if (h.IsValid()) Addressables.ReleaseInstance(h);
            _handles.Clear();
        }

        private static Vector3 GridToWorld(Vector2Int pos, float cellSize)
            => new(pos.x * cellSize, 0f, pos.y * cellSize);

        private static Vector3 BoxGridToWorld(Vector2Int pos, AddressableGroupConfig cfg)
            => new(pos.x * cfg.CellSize, 0f, cfg.BoxGridYOffset + pos.y * cfg.CellSize);

        private static Vector3 ConveyorToWorld(Vector2 local, float cellSize)
            => new(local.x * cellSize, 0f, local.y * cellSize);

        private static void ApplyTeam(GameObject go, Team team)
        {
            var box = go.GetComponent<BoxController>();
            box?.AssignTeam(team);
        }
    }

    public interface ILevelInstantiator
    {
        void ReleaseAll();
    }
}
