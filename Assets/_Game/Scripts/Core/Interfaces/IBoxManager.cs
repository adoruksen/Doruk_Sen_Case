using System;
using RubyCase.Gameplay.BoxSystem;
using UnityEngine;

namespace RubyCase.Core.Session
{
    public interface IBoxManager
    {
        int Total { get; }
        int Remaining { get; }
        bool IsCleared { get; }
        
        void InitializeFromLevel(GameObject[] boxes);
        void MarkResolved(GameObject box);
        void Reset();
        void SetupGrid(int cols, int rows, float pitch);
        void RegisterBox(BoxController box, int col, int row);
        void OnBoxDeparted(BoxController box);
    }
}
