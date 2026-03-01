using System;
using RubyCase.Core.Level;
using UnityEngine;
using Zenject;

namespace RubyCase.Core.Session
{
    public sealed class ConveyorManager : IConveyorManager, IInitializable, IDisposable
    {
        public bool IsReady => Waypoints != null;
        public Vector3[] Waypoints { get; private set; }
        public int RoadStartIndex => 0;

        private readonly ILevelManager _levelManager;
        private readonly LevelCreationSettings _settings;

        private int _gridSize;

        public ConveyorManager(ILevelManager levelManager, LevelCreationSettings settings)
        {
            _levelManager = levelManager;
            _settings = settings;
        }

        public void Initialize()
        {
            _levelManager.OnLevelReady += OnLevelReady;
            _levelManager.OnLevelCleared += OnLevelCleared;
        }

        public void Dispose()
        {
            _levelManager.OnLevelReady -= OnLevelReady;
            _levelManager.OnLevelCleared -= OnLevelCleared;
        }

        private void OnLevelReady()
        {
            var data = _levelManager.CurrentData;
            var origin = _levelManager.CurrentContext.CollectablesBottomLeft;

            _gridSize = Mathf.Max(1, data.collectableGridWidth);
            Waypoints = BuildWaypoints(_gridSize, origin, _settings);
        }

        private void OnLevelCleared()
        {
            Waypoints = null;
            _gridSize = 0;
        }

        public ScanInfo? GetScanInfo(int i)
        {
            int n = _gridSize;

            if (i < n) return new ScanInfo { IsColumn = true, LineIndex = n - 1 - i, FromNear = true };
            if (i == n) return null;
            if (i <= 2 * n) return new ScanInfo { IsColumn = false, LineIndex = i - n - 1, FromNear = true };
            if (i == 2 * n + 1) return null;
            if (i <= 3 * n + 1) return new ScanInfo { IsColumn = true, LineIndex = i - 2 * n - 2, FromNear = false };
            if (i == 3 * n + 2) return null;
            if (i <= 4 * n + 2)
                return new ScanInfo { IsColumn = false, LineIndex = n - 1 - (i - 3 * n - 3), FromNear = false };

            return null;
        }

        private static Vector3[] BuildWaypoints(int n, Vector3 origin, LevelCreationSettings s)
        {
            float pitch = s.GetCellPitch(n);
            float cellSize = s.GetCellSize(n);
            float y = s.WaypointY;

            float edgeDist = cellSize * 0.5f + s.ConveyorGridGap - s.WaypointInset;

            float bottomZ = origin.z - edgeDist;
            float topZ = origin.z + n * pitch + edgeDist;
            float leftX = origin.x - edgeDist;
            float rightX = origin.x + n * pitch + edgeDist;

            float CellCenterX(int col) => origin.x + col * pitch + pitch * 0.5f;
            float CellCenterZ(int row) => origin.z + row * pitch + pitch * 0.5f;

            var pts = new Vector3[4 * n + 4];
            int idx = 0;

            for (int col = n - 1; col >= 0; col--)
                pts[idx++] = new Vector3(CellCenterX(col), y, bottomZ);
            pts[idx++] = new Vector3(leftX, y, bottomZ);

            for (int row = 0; row < n; row++)
                pts[idx++] = new Vector3(leftX, y, CellCenterZ(row));
            pts[idx++] = new Vector3(leftX, y, topZ);

            for (int col = 0; col < n; col++)
                pts[idx++] = new Vector3(CellCenterX(col), y, topZ);
            pts[idx++] = new Vector3(rightX, y, topZ);

            for (int row = n - 1; row >= 0; row--)
                pts[idx++] = new Vector3(rightX, y, CellCenterZ(row));
            pts[idx] = new Vector3(rightX, y, bottomZ);

            return pts;
        }
    }
}