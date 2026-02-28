using System;
using System.Collections.Generic;
using RubyCase.Core;
using RubyCase.StateMachine;
using RubyCase.TeamSystem;
using Sirenix.OdinInspector;
using UnityEngine;
using Zenject;

namespace RubyCase.BoxSystem
{
    public class BoxController : MonoBehaviour, IHaveTeam, IClickable
    {
        public event Action<Team> OnTeamChanged;
        public event Action<int> OnNodeReached;
        public event Action OnPathCompleted;
        public event Action OnBenchArrived;
        public event Action OnFullyLoaded;
        public event Action OnDestroyed;

        [ShowInInspector, ReadOnly] public Team Team { get; private set; }
        [ShowInInspector, ReadOnly] public int Current { get; private set; }

        public int Capacity => _slots.Count;
        public bool IsFull => Current >= Capacity;
        private int _occupiedCount;


        [SerializeField] private List<BoxSlot> _slots = new();
        public bool IsClickable { get; private set; }

        public IReadOnlyList<BoxSlot> Slots => _slots;

        public StateManager<BoxController> StateMachine { get; private set; }

        public readonly BoxIdleState IdleState = new();
        public readonly BoxMoveToBenchState MoveToBenchState = new();
        public readonly BoxOnBenchState OnBenchState = new();
        public readonly BoxMoveToConveyorState MoveToConveyorState = new();
        public readonly BoxOnConveyorState OnConveyorState = new();

        [Inject] private IBoxJourneyService _journeyService;

        private void Awake()
        {
            StateMachine = new StateManager<BoxController>(this);
            StateMachine.TransitionTo(IdleState);
        }

        private void Update() => StateMachine.Tick();

        private void OnDestroy() => OnDestroyed?.Invoke();

        public void OnClicked()
        {
            if (_journeyService.CanStartJourney(this))
                _journeyService.StartJourney(this);
        }

        public void SetClickable(bool clickable)
        {
            IsClickable = clickable;
        }

        public void AssignTeam(Team team)
        {
            if (Team == team) return;
            Team = team;
            OnTeamChanged?.Invoke(Team);
        }

        public BoxSlot GetAvailableSlot()
        {
            foreach (var slot in _slots)
                if (slot.IsAvailable)
                    return slot;
            return null;
        }

        public void Collect(BoxSlot slot, GameObject collectable)
        {
            if (IsFull) return;
            slot.Reserve(collectable);
            Current++;
        }

        public void NotifySlotOccupied()
        {
            _occupiedCount++;
            if (_occupiedCount >= Capacity)
                OnFullyLoaded?.Invoke();
        }

        public void NotifyNodeReached(int index) => OnNodeReached?.Invoke(index);
        public void NotifyPathCompleted() => OnPathCompleted?.Invoke();
        public void NotifyBenchArrived() => OnBenchArrived?.Invoke();
    }
}