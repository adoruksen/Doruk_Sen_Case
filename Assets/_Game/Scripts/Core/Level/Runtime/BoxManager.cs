using System;
using System.Collections.Generic;
using DG.Tweening;
using RubyCase.BoxSystem;
using UnityEngine;

namespace RubyCase.Core
{
    public sealed class BoxManager : IBoxManager
    {
        public int Total { get; private set; }
        public int Remaining { get; private set; }
        public bool IsCleared => Total > 0 && Remaining <= 0;

        public event Action Cleared;
        
        private BoxController[,] _grid;
        private readonly Dictionary<BoxController, Vector2Int> _positions = new();
        private int _cols, _rows;
        private float _pitch;

        public void SetupGrid(int cols, int rows, float pitch)
        {
            _grid = new BoxController[cols, rows];
            _cols = cols;
            _rows = rows;
            _pitch = pitch;
            _positions.Clear();
        }

        public void RegisterBox(BoxController box, int col, int row)
        {
            if (_grid == null || col < 0 || col >= _cols || row < 0 || row >= _rows) return;
            _grid[col, row] = box;
            _positions[box] = new Vector2Int(col, row);
        }

        public void OnBoxDeparted(BoxController box)
        {
            if (_grid == null || !_positions.TryGetValue(box, out var pos)) return;

            int col = pos.x;
            int row = pos.y;

            _positions.Remove(box);
            _grid[col, row] = null;

            for (int r = row - 1; r >= 0; r--)
            {
                var behind = _grid[col, r];
                if (behind == null) continue;

                _grid[col, r + 1] = behind;
                _grid[col, r] = null;
                _positions[behind] = new Vector2Int(col, r + 1);

                var target = behind.transform.position + new Vector3(0f, 0f, _pitch);
                behind.transform.DOMove(target, 0.25f).SetEase(Ease.OutCubic);
            }
        }
        
        public void InitializeFromLevel(GameObject[] boxes)
        {
            Reset();
            Total = boxes?.Length ?? 0;
            Remaining = Total;
            if (Total == 0) Cleared?.Invoke();
        }

        public void MarkResolved(GameObject box)
        {
            if (box == null || Remaining <= 0) return;
            Remaining--;
            if (Remaining == 0) Cleared?.Invoke();
        }

        public void Reset()
        {
            Total = 0;
            Remaining = 0;
            _grid = null;
            _cols = _rows = 0;
            _pitch = 0f;
            _positions.Clear();
        }
    }
}