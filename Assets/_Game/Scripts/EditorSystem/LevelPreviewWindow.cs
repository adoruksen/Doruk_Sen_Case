#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using RubyCase.TeamSystem;

namespace RubyCase.LevelSystem.Editor
{
    public class LevelPreviewWindow : EditorWindow
    {
        private LevelData _level;
        private Vector2   _scroll;

        private static readonly Color BgWindow      = new(0.13f, 0.13f, 0.15f);
        private static readonly Color BgUpper       = new(0.09f, 0.09f, 0.11f);
        private static readonly Color BgLower       = new(0.11f, 0.08f, 0.05f);
        private static readonly Color CellEmpty     = new(0.20f, 0.20f, 0.23f);
        private static readonly Color CellBoxEmpty  = new(0.22f, 0.17f, 0.12f);
        private static readonly Color CellConveyor  = new(0.17f, 0.33f, 0.68f);
        private static readonly Color Outline       = new(0f, 0f, 0f, 0.55f);

        private GUIStyle _styleCenter;
        private GUIStyle StyleCenter => _styleCenter ??= new GUIStyle(EditorStyles.boldLabel)
            { alignment = TextAnchor.MiddleCenter, fontSize = 9, normal = { textColor = Color.white } };

        private GUIStyle _styleSmall;
        private GUIStyle StyleSmall => _styleSmall ??= new GUIStyle(EditorStyles.miniLabel)
            { alignment = TextAnchor.MiddleCenter, fontSize = 7, normal = { textColor = new Color(1f, 1f, 1f, 0.55f) } };

        public static void Show(LevelData level)
        {
            var win = GetWindow<LevelPreviewWindow>(false, $"Preview — {level.name}", true);
            win._level = level;
            win.minSize = new Vector2(300, 400);
            win.Repaint();
        }

        private void OnGUI()
        {
            if (_level == null) { GUILayout.Label("No level loaded."); return; }

            EditorGUI.DrawRect(new Rect(0, 0, position.width, position.height), BgWindow);
            _scroll = EditorGUILayout.BeginScrollView(_scroll);
            GUILayout.Space(6);

            DrawHeader();
            GUILayout.Space(6);
            DrawUpperGrid();
            GUILayout.Space(4);
            DrawLowerGrid();
            GUILayout.Space(6);
            DrawStats();

            GUILayout.Space(8);
            EditorGUILayout.EndScrollView();
        }

        private void DrawHeader()
        {
            using (new GUILayout.HorizontalScope())
            {
                GUILayout.Label($"{_level.name}  —  ID {_level.levelID}",
                    new GUIStyle(EditorStyles.boldLabel) { fontSize = 12, normal = { textColor = Color.white } });
            }
            GUILayout.Label($"Grid {_level.collectableGridWidth}×{_level.collectableGridHeight}   " +
                            $"Boxes {_level.boxGridWidth}×{_level.boxGridHeight}   " +
                            $"Bench {_level.benchCapacity}",
                EditorStyles.miniLabel);
        }

        private void DrawUpperGrid()
        {
            GUILayout.Label("Collectable Grid + Conveyor Ring",
                new GUIStyle(EditorStyles.boldLabel) { fontSize = 10, normal = { textColor = new Color(0.7f, 0.7f, 0.75f) } });
            GUILayout.Space(2);

            if (_level.collectableCells == null || _level.collectableCells.Count == 0)
            {
                EditorGUILayout.HelpBox("Grid not initialized.", MessageType.Info);
                return;
            }

            int W = _level.collectableGridWidth;
            int H = _level.collectableGridHeight;
            int totalCols = W + 2;
            int totalRows = H + 2;

            float cs = CalculateCellSize(totalCols, totalRows);

            Rect canvas = GUILayoutUtility.GetRect(totalCols * cs, totalRows * cs);
            EditorGUI.DrawRect(canvas, BgUpper);

            for (int cy = 0; cy < totalRows; cy++)
            {
                for (int cx = 0; cx < totalCols; cx++)
                {
                    Rect cr = new(canvas.x + cx * cs, canvas.y + cy * cs, cs - 1, cs - 1);
                    bool isRing = cx == 0 || cx == totalCols - 1 || cy == 0 || cy == totalRows - 1;

                    if (isRing)
                    {
                        bool isCorner = (cx == 0 || cx == totalCols - 1) && (cy == 0 || cy == totalRows - 1);
                        EditorGUI.DrawRect(cr, isCorner ? new Color(0.12f, 0.25f, 0.55f) : CellConveyor);
                        DrawOutline(cr);
                        continue;
                    }

                    int gx = cx - 1;
                    int gy = (H - 1) - (cy - 1);
                    var cell = _level.GetCollectableCell(gx, gy);
                    if (cell == null) continue;

                    Color bg = cell.isFilled && cell.team != null ? cell.team.EditorColor : CellEmpty;
                    EditorGUI.DrawRect(cr, bg);
                    DrawOutline(cr);

                    if (cell.isFilled && cs >= 12)
                        GUI.Label(cr, cell.team?.EditorSymbol ?? "?", StyleCenter);
                }
            }
        }

        private void DrawLowerGrid()
        {
            GUILayout.Label("Box Grid",
                new GUIStyle(EditorStyles.boldLabel) { fontSize = 10, normal = { textColor = new Color(0.7f, 0.7f, 0.75f) } });
            GUILayout.Space(2);

            if (_level.boxCells == null || _level.boxCells.Count == 0)
            {
                EditorGUILayout.HelpBox("Box grid not initialized.", MessageType.Info);
                return;
            }

            int W = _level.boxGridWidth;
            int H = _level.boxGridHeight;
            float cs = CalculateCellSize(W, H);

            Rect canvas = GUILayoutUtility.GetRect(W * cs, H * cs);
            EditorGUI.DrawRect(canvas, BgLower);

            for (int cy = 0; cy < H; cy++)
            {
                for (int cx = 0; cx < W; cx++)
                {
                    int gy = (H - 1) - cy;
                    Rect cr = new(canvas.x + cx * cs, canvas.y + cy * cs, cs - 1, cs - 1);
                    var cell = _level.GetBoxCell(cx, gy);
                    if (cell == null) continue;

                    Color bg = cell.isFilled && cell.team != null ? cell.team.EditorColor : CellBoxEmpty;
                    EditorGUI.DrawRect(cr, bg);
                    DrawOutline(cr);

                    if (cell.isFilled && cs >= 12)
                    {
                        string lbl = (cell.team?.EditorSymbol ?? "?")
                            + (cell.capacityOverride > 0 ? $"\n{cell.capacityOverride}" : "");
                        GUI.Label(cr, lbl, StyleCenter);
                    }
                }
            }
        }

        private void DrawStats()
        {
            var result = LevelValidator.Validate(_level);

            Color statusColor = result.IsValid ? new Color(0.3f, 0.8f, 0.4f) : new Color(0.9f, 0.3f, 0.3f);
            string statusText = result.IsValid ? "✓  Valid" : $"✗  {result.Errors.Count} error(s)";

            GUILayout.Label(statusText, new GUIStyle(EditorStyles.boldLabel)
                { fontSize = 11, normal = { textColor = statusColor } });

            foreach (var e in result.Errors)
                GUILayout.Label($"  ✗ {e}", new GUIStyle(EditorStyles.miniLabel) { normal = { textColor = new Color(1f, 0.45f, 0.45f) } });
            foreach (var w in result.Warnings)
                GUILayout.Label($"  ⚠ {w}", new GUIStyle(EditorStyles.miniLabel) { normal = { textColor = new Color(1f, 0.85f, 0.35f) } });

            GUILayout.Space(4);
            GUILayout.Label($"Collectables: {_level.TotalCollectables}   Boxes: {_level.TotalBoxes}", EditorStyles.miniLabel);
        }

        private float CalculateCellSize(int cols, int rows)
        {
            float maxW = (position.width - 20f) / cols;
            float maxH = 240f / rows;
            return Mathf.Floor(Mathf.Min(maxW, maxH, EditorConstants.CellSize));
        }

        private static void DrawOutline(Rect r)
        {
            Handles.BeginGUI();
            Handles.DrawSolidRectangleWithOutline(r, Color.clear, Outline);
            Handles.EndGUI();
        }
    }
}
#endif