using System;
using UnityEngine;

namespace RubyCase.Core
{
    public interface IBoxManager
    {
        int Total { get; }
        int Remaining { get; }
        bool IsCleared { get; }

        event Action Cleared;

        void InitializeFromLevel(GameObject[] boxes);
        void MarkResolved(GameObject box);
        void Reset();
    }
}
