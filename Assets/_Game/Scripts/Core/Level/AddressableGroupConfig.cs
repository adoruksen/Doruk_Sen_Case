using UnityEngine;
using UnityEngine.AddressableAssets;
using Sirenix.OdinInspector;

namespace RubyCase.Core
{
    [CreateAssetMenu(fileName = "AddressableGroupConfig", menuName = "RubyCase/Addressable Group Config")]
    public class AddressableGroupConfig : ScriptableObject
    {
        public AssetReferenceGameObject CollectablePrefab;
        public AssetReferenceGameObject BoxPrefab;
        public AssetReferenceGameObject ConveyorPrefab;
        public AssetReferenceGameObject BenchPrefab;
    }
}