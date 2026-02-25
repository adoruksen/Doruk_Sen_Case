using RubyCase.TeamSystem;
using UnityEngine;

namespace RubyCase.Data
{
    [CreateAssetMenu(fileName = "BoxData_New",menuName = "GameData/LevelDatas/Box")]
    public sealed class BoxData : ScriptableObject
    {
        [SerializeField] private Team team;

        [SerializeField, Min(1)] private int defaultCapacity = 4;
        
    }

}
