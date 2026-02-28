using System;
using RubyCase.BoxSystem;
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
        void SetupGrid(int cols, int rows, float pitch);
        void RegisterBox(BoxController box, int col, int row);
        void OnBoxDeparted(BoxController box);
    }
}
