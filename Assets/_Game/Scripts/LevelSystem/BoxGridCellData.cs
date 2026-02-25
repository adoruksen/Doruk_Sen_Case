using System;
using UnityEngine;
using RubyCase.TeamSystem;

namespace RubyCase.LevelSystem
{
    [Serializable]
    public class BoxGridCellData
    {
        public Vector2Int position;
        public bool isFilled;
        public Team team;
        public int capacityOverride;

        public BoxGridCellData(Vector2Int pos)
        {
            position = pos;
            isFilled = false;
        }

        public void SetBox(Team boxTeam, int capacity = 0)
        {
            isFilled         = true;
            team             = boxTeam;
            capacityOverride = capacity;
        }

        public void Clear()
        {
            isFilled         = false;
            team             = null;
            capacityOverride = 0;
        }
    }
}
