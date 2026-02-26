using UnityEngine;
using UnityEngine.AddressableAssets;
using Sirenix.OdinInspector;

namespace RubyCase.Core
{
    [CreateAssetMenu(fileName = "AddressableGroupConfig", menuName = "RubyCase/Addressable Group Config")]
    public class AddressableGroupConfig : ScriptableObject
    {
        [Title("Prefabs")]
        public AssetReferenceGameObject CollectablePrefab;
        public AssetReferenceGameObject BoxPrefab;
        public AssetReferenceGameObject ConveyorNodePrefab;

        [Title("Grid")] 
        public float CellSize = 1f;
        public float BoxGridYOffset = -4f;
    }
}
