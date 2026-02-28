using System;
using UnityEngine;

namespace RubyCase.Core
{
    public sealed class BoxManager : IBoxManager
    {
        public int Total { get; private set; }
        public int Remaining { get; private set; }
        public bool IsCleared => Total > 0 && Remaining <= 0;

        public event Action Cleared;

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
            if (box == null || Remaining <= 0) return;

            Remaining--;

            if (Remaining == 0)
                Cleared?.Invoke();
        }

        public void Reset()
        {
            Total = 0;
            Remaining = 0;
        }
    }
}