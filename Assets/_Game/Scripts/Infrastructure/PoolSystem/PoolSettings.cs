using UnityEngine;
using Sirenix.OdinInspector;

namespace RubyCase.Pool
{
    [CreateAssetMenu(fileName = "PoolSettings", menuName = "RubyCase/Pool Settings")]
    public class PoolSettings : ScriptableObject
    {
        [Min(0)] public int CollectablePrewarm = 32;

        [Min(0)] public int BoxPrewarm = 12;


        [Min(0)] public int BenchPrewarm = 6;
    }
}