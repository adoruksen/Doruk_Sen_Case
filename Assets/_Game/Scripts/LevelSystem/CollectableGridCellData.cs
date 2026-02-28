using System;
using UnityEngine;
using RubyCase.TeamSystem;

namespace RubyCase.LevelSystem
{
    [Serializable]
    public class CollectableGridCellData
    {
        public Vector2Int position;
        public bool isFilled;
        public Team team;
        [NonSerialized] public GameObject SpawnedObject;

        public CollectableGridCellData(Vector2Int pos)
        {
            position = pos;
        }

        public void SetCollectable(Team assignedTeam)
        {
            isFilled = true;
            team = assignedTeam;
        }

        public void Clear()
        {
            isFilled = false;
            team = null;
            SpawnedObject = null;
        }
    }
}