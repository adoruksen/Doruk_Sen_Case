using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;

namespace RubyCase.Pool
{
    public sealed class AddressablePool : IDisposable
    {
        private AsyncOperationHandle<GameObject> _loadHandle;
        private GameObject _prefab;
        private bool _initialized;

        private readonly Queue<PoolEntry> _available = new();
        private readonly Dictionary<GameObject, PoolEntry> _active = new();

        private Transform _poolRoot;
        private string _assetKey;

        private sealed class PoolEntry
        {
            public readonly GameObject Go;
            public readonly IPoolable[] Poolables;

            public PoolEntry(GameObject go)
            {
                Go = go;
                Poolables = go.GetComponentsInChildren<IPoolable>(includeInactive: true);
            }
        }

        public async UniTask InitializeAsync(AssetReferenceGameObject assetRef, int prewarmCount, Transform poolRoot)
        {
            if (_initialized) return;

            _poolRoot = poolRoot;
            _assetKey = assetRef.AssetGUID;

            _loadHandle = Addressables.LoadAssetAsync<GameObject>(assetRef);
            await _loadHandle;

            if (_loadHandle.Status != AsyncOperationStatus.Succeeded)
            {
                Debug.LogError(
                    $"[AddressablePool] Failed to load asset '{assetRef.RuntimeKey}'. Pool will not function.");
                return;
            }

            _prefab = _loadHandle.Result;
            _initialized = true;

            for (int i = 0; i < prewarmCount; i++)
                CreateAndStash();
        }

        public void AddInstances(int count)
        {
            if (!_initialized) return;
            for (int i = 0; i < count; i++)
                CreateAndStash();
        }

        public GameObject Spawn(Vector3 position, Quaternion rotation, Transform parent = null)
        {
            if (!_initialized)
            {
                Debug.LogError($"[AddressablePool:{_assetKey}] Spawn called before InitializeAsync completed.");
                return null;
            }

            var entry = _available.Count > 0 ? _available.Dequeue() : CreateEntry();

            var go = entry.Go;
            go.transform.SetParent(parent != null ? parent : _poolRoot, worldPositionStays: false);
            go.transform.SetPositionAndRotation(position, rotation);
            go.SetActive(true);

            _active[go] = entry;

            NotifySpawn(entry);
            return go;
        }

        public bool Release(GameObject instance)
        {
            if (instance == null) return false;
            if (!_active.TryGetValue(instance, out var entry)) return false;

            _active.Remove(instance);
            NotifyDespawn(entry);

            instance.SetActive(false);
            instance.transform.SetParent(_poolRoot, worldPositionStays: false);
            _available.Enqueue(entry);

            return true;
        }

        public void Dispose()
        {
            foreach (var kv in _active)
            {
                if (kv.Key == null) continue;
                NotifyDespawn(kv.Value);
                UnityEngine.Object.Destroy(kv.Key);
            }

            _active.Clear();

            while (_available.Count > 0)
            {
                var entry = _available.Dequeue();
                if (entry.Go != null) UnityEngine.Object.Destroy(entry.Go);
            }

            if (_loadHandle.IsValid())
                Addressables.Release(_loadHandle);

            _initialized = false;
            _prefab = null;
        }

        private PoolEntry CreateEntry()
        {
            var go = UnityEngine.Object.Instantiate(_prefab, _poolRoot);
            go.SetActive(false);
            return new PoolEntry(go);
        }

        private void CreateAndStash() => _available.Enqueue(CreateEntry());

        private static void NotifySpawn(PoolEntry entry)
        {
            foreach (var p in entry.Poolables)
                p.OnSpawn();
        }

        private static void NotifyDespawn(PoolEntry entry)
        {
            foreach (var p in entry.Poolables)
                p.OnDespawn();
        }

        public int ActiveCount => _active.Count;
        public int AvailableCount => _available.Count;
        public bool IsInitialized => _initialized;
    }
}