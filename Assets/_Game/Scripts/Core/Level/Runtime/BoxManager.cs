using System;
using System.Collections.Generic;
using UnityEngine;

namespace RubyCase.Core
{
    public sealed class BoxManager : IBoxManager
    {
        public int Total { get; private set; }
        public int Remaining { get; private set; }
        public bool IsCleared => Remaining <= 0 && Total >= 0;

        public event Action Cleared;

        private readonly HashSet<int> _resolvedInstanceIds = new();

        public void InitializeFromLevel(GameObject[] boxes)
        {
            Reset();

            Total = boxes?.Length ?? 0;
            Remaining = Total;

            if (Total == 0)
                Cleared?.Invoke();
        }

        public void MarkResolved(GameObject box)
        {
            if (box == null) return;
            if (Total <= 0) return;

            int id = box.GetInstanceID();
            if (!_resolvedInstanceIds.Add(id)) return;

            Remaining = Mathf.Max(0, Remaining - 1);

            if (Remaining == 0)
                Cleared?.Invoke();
        }

        public void Reset()
        {
            _resolvedInstanceIds.Clear();
            Total = 0;
            Remaining = 0;
        }
    }
}
