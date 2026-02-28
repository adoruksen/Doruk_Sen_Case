using RubyCase.LevelSystem;
using UnityEngine;

namespace RubyCase.BoxSystem
{
    public interface IConveyorManager
    {
        ConveyorPathData Path { get; }
        Vector3 GridWorldOrigin { get; }
        bool IsReady { get; }
    }
}