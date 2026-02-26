using System;
using UnityEngine;

namespace RubyCase.Core
{
    public interface ICollectableManager
    {
        int Total { get; }
        int Remaining { get; }
        bool IsCleared { get; }

        event Action Cleared;

        void InitializeFromLevel(GameObject[] collectables);
        void MarkResolved(GameObject collectable);
        void Reset();
    }
}
