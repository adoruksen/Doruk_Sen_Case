using UnityEngine;

namespace RubyCase.Core.Session
{
    public struct ScanInfo
    {
        public bool IsColumn;
        public int  LineIndex;
        public bool FromNear;
    }

    public interface IConveyorManager
    {
        bool IsReady { get; }
        Vector3[] Waypoints { get; }
        int RoadStartIndex { get; }

        ScanInfo? GetScanInfo(int waypointIndex);
    }
}