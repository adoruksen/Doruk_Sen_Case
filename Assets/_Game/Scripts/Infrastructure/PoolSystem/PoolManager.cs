using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using RubyCase.Core;
using RubyCase.Core.Level;
using UnityEngine;
using UnityEngine.AddressableAssets;
using Zenject;

namespace RubyCase.Pool
{
    public sealed class PoolManager : IPoolManager, IInitializable, IDisposable
    {
        private readonly AddressableGroupConfig _groupConfig;
        private readonly PoolSettings _settings;
        private readonly ILevelManager _levelManager;

        private readonly Dictionary<string, AddressablePool> _pools = new();
        private readonly Dictionary<string, UniTaskCompletionSource<AddressablePool>> _initSources = new();
        private readonly Dictionary<GameObject, AddressablePool> _instanceMap = new();

        private Transform _poolRoot;

        public PoolManager(AddressableGroupConfig groupConfig, PoolSettings settings, ILevelManager levelManager)
        {
            _groupConfig = groupConfig;
            _settings = settings;
            _levelManager = levelManager;
        }

        public void Initialize()
        {
            var rootGO = new GameObject("[PoolRoot]");
            UnityEngine.Object.DontDestroyOnLoad(rootGO);
            _poolRoot = rootGO.transform;
            _levelManager.OnSpawnRequested += HandleSpawnRequested;
        }

        public void Dispose()
        {
            _levelManager.OnSpawnRequested -= HandleSpawnRequested;
            ReleaseAll();
        }

        private void HandleSpawnRequested(LevelSystem.LevelData _) => PrewarmLevelPoolsAsync().Forget();

        private async UniTaskVoid PrewarmLevelPoolsAsync()
        {
            await UniTask.WhenAll(
                PrewarmAsync(_groupConfig.CollectablePrefab, _settings.CollectablePrewarm),
                PrewarmAsync(_groupConfig.BoxPrefab, _settings.BoxPrewarm),
                PrewarmAsync(_groupConfig.BenchPrefab, _settings.BenchPrewarm)
            );
        }

        public async UniTask PrewarmAsync(AssetReferenceGameObject assetRef, int count)
        {
            if (!IsValidRef(assetRef) || count <= 0) return;
            string key = assetRef.AssetGUID;

            if (_initSources.TryGetValue(key, out var existingTcs))
            {
                var pool = await existingTcs.Task;
                int missing = count - pool.AvailableCount;
                if (missing > 0) pool.AddInstances(missing);
                return;
            }

            var tcs = new UniTaskCompletionSource<AddressablePool>();
            _initSources[key] = tcs;
            await DoInitPoolAsync(assetRef, count, tcs);
        }

        public async UniTask<GameObject> SpawnAsync(AssetReferenceGameObject assetRef,
            Vector3 position, Quaternion rotation, Transform parent = null)
        {
            if (!IsValidRef(assetRef))
            {
                Debug.LogWarning("[PoolManager] SpawnAsync: null/invalid AssetReference.");
                return null;
            }

            var pool = await GetOrCreatePoolAsync(assetRef);
            var go = pool.Spawn(position, rotation, parent);

            if (go != null)
                _instanceMap[go] = pool;

            return go;
        }

        public void Release(GameObject instance)
        {
            if (instance == null) return;
            if (!_instanceMap.TryGetValue(instance, out var pool)) return;

            _instanceMap.Remove(instance);
            pool.Release(instance);
        }

        public void ReleaseAll()
        {
            _instanceMap.Clear();
            foreach (var pool in _pools.Values) pool.Dispose();
            _pools.Clear();
            _initSources.Clear();
            if (_poolRoot != null) UnityEngine.Object.Destroy(_poolRoot.gameObject);
        }

        private UniTask<AddressablePool> GetOrCreatePoolAsync(AssetReferenceGameObject assetRef)
        {
            string key = assetRef.AssetGUID;
            if (_initSources.TryGetValue(key, out var existing)) return existing.Task;

            var tcs = new UniTaskCompletionSource<AddressablePool>();
            _initSources[key] = tcs;
            DoInitPoolAsync(assetRef, prewarmCount: 0, tcs).Forget();
            return tcs.Task;
        }

        private async UniTask DoInitPoolAsync(
            AssetReferenceGameObject assetRef, int prewarmCount,
            UniTaskCompletionSource<AddressablePool> tcs)
        {
            try
            {
                var child = new GameObject($"[Pool] {assetRef.RuntimeKey}");
                child.transform.SetParent(_poolRoot, worldPositionStays: false);
                var pool = new AddressablePool();
                await pool.InitializeAsync(assetRef, prewarmCount, child.transform);
                _pools[assetRef.AssetGUID] = pool;
                tcs.TrySetResult(pool);
            }
            catch (Exception e)
            {
                tcs.TrySetException(e);
            }
        }

        private static bool IsValidRef(AssetReferenceGameObject reference) =>
            reference != null && reference.RuntimeKeyIsValid();
    }
}