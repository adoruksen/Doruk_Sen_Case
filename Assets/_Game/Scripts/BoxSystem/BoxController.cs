using System;
using System.Collections.Generic;
using RubyCase.TeamSystem;
using Sirenix.OdinInspector;
using UnityEngine;

namespace RubyCase.BoxSystem
{
    public class BoxController : MonoBehaviour,IHaveTeam
    {
        public event Action<Team> OnTeamChanged;
        [ShowInInspector,ReadOnly] public Team Team { get; private set; }
        
        [ShowInInspector, ReadOnly] public int  Current { get; private set; }
        public int  Capacity => _slots.Count;
        public bool IsFull   => Current >= Capacity;

        [SerializeField] private List<BoxSlot> _slots = new();

        public IReadOnlyList<BoxSlot> Slots => _slots;

        public BoxSlot GetAvailableSlot()
        {
            foreach (var slot in _slots)
                if (slot.IsAvailable) return slot;
            return null;
        }
        
        public void Collect(BoxSlot slot, GameObject collectable)
        {
            if (IsFull) return;
            slot.Reserve(collectable);
            Current++;
        }

        public void AssignTeam(Team team)
        {
            if(Team == team) return;
            Team = team;
            OnTeamChanged?.Invoke(Team);
        }
    }

}
