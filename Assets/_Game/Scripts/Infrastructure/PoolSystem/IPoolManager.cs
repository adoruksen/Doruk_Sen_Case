using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.AddressableAssets;

namespace RubyCase.Pool
{
    public interface IPoolManager
    { 
        UniTask PrewarmAsync(AssetReferenceGameObject assetRef, int count);
        UniTask<GameObject> SpawnAsync(AssetReferenceGameObject assetRef, Vector3 position, Quaternion rotation, Transform parent = null);
        void Release(GameObject instance);
        void ReleaseAll();
    }
}