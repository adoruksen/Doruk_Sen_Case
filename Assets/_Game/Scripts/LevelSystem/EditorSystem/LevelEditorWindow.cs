#if UNITY_EDITOR
using System.IO;
using System.Linq;
using UnityEngine;
using UnityEditor;
using RubyCase.TeamSystem;

namespace RubyCase.LevelSystem.Editor
{
    public class LevelEditorWindow : EditorWindow
    {
        [MenuItem("RubyCase/Level Editor")]
        private static void Open()
        {
            var win = GetWindow<LevelEditorWindow>(false, "Level Editor", true);
            win.minSize = new Vector2(340, 500);
            win.Show();
        }

        private enum WindowState
        {
            NoLevel,
            LevelLoaded
        }

        private WindowState _state = WindowState.NoLevel;
        private LevelData _level;
        private TeamDatabase _teamDb;
        private int _teamIndex;
        private EditorBrushType _brush = EditorBrushType.Collectable;
        private int _boxCap;
        private Vector2 _scroll;

        private bool _isDragging;
        private int _dragButton;

        private static readonly Color BgWindow = new(0.13f, 0.13f, 0.15f);
        private static readonly Color BgCanvasUpper = new(0.09f, 0.09f, 0.11f);
        private static readonly Color BgCanvasLower = new(0.11f, 0.08f, 0.05f);
        private static readonly Color CellEmpty = new(0.20f, 0.20f, 0.23f);
        private static readonly Color CellBoxEmpty = new(0.22f, 0.17f, 0.12f);
        private static readonly Color CellConveyor = new(0.17f, 0.33f, 0.68f);
        private static readonly Color CellConvAligned = new(0.20f, 0.44f, 0.88f);
        private static readonly Color CellConvStart = new(0.18f, 0.65f, 0.28f);
        private static readonly Color CellConvCorner = new(0.12f, 0.25f, 0.55f);
        private static readonly Color Outline = new(0f, 0f, 0f, 0.55f);
        private static readonly Color SepColor = new(1f, 1f, 1f, 0.07f);
        private static readonly Color BrushOn_C = new(0.20f, 0.60f, 0.30f);
        private static readonly Color BrushOn_B = new(0.78f, 0.50f, 0.13f);
        private static readonly Color BrushOn_X = new(0.42f, 0.42f, 0.44f);
        private static readonly Color BrushOff = new(0.27f, 0.27f, 0.29f);

        private GUIStyle _styleCenter;
        private GUIStyle _styleSmall;
        private GUIStyle _styleSection;

        private GUIStyle StyleCenter => _styleCenter ??= new GUIStyle(EditorStyles.boldLabel)
            { alignment = TextAnchor.MiddleCenter, fontSize = 9, normal = { textColor = Color.white } };

        private GUIStyle StyleSmall => _styleSmall ??= new GUIStyle(EditorStyles.miniLabel)
        {
            alignment = TextAnchor.MiddleCenter, fontSize = 7, normal = { textColor = new Color(1f, 1f, 1f, 0.65f) }
        };

        private GUIStyle StyleSection => _styleSection ??= new GUIStyle(EditorStyles.boldLabel)
            { fontSize = 10, normal = { textColor = new Color(0.72f, 0.72f, 0.75f) } };

        private void OnGUI()
        {
            TrackDragState(Event.current);

            EditorGUI.DrawRect(new Rect(0, 0, position.width, position.height), BgWindow);
            _scroll = EditorGUILayout.BeginScrollView(_scroll);
            if (_state == WindowState.NoLevel) DrawNoLevel();
            else DrawLevelLoaded();
            EditorGUILayout.EndScrollView();
        }

        private void TrackDragState(Event e)
        {
            if (e.type == EventType.MouseDown && (e.button == 0 || e.button == 2))
            {
                _isDragging = true;
                _dragButton = e.button;
            }
            else if (e.type == EventType.MouseUp)
            {
                _isDragging = false;
            }
        }
        
        private bool ShouldPaint(Rect cr, Event e, out int button)
        {
            button = -1;
            if (!cr.Contains(e.mousePosition)) return false;

            if (e.type == EventType.MouseDown && (e.button == 0 || e.button == 1 || e.button == 2))
            {
                button = e.button;
                return true;
            }

            if (e.type == EventType.MouseDrag && _isDragging && _dragButton != 1)
            {
                button = _dragButton;
                return true;
            }

            return false;
        }
        
        private void DrawNoLevel()
        {
            GUILayout.FlexibleSpace();
            using (new GUILayout.HorizontalScope())
            {
                GUILayout.FlexibleSpace();
                using (new GUILayout.VerticalScope(GUILayout.Width(220)))
                {
                    GUILayout.Label("Level Editor", new GUIStyle(EditorStyles.boldLabel)
                    {
                        fontSize = 15, alignment = TextAnchor.MiddleCenter,
                        normal = { textColor = Color.white }
                    }, GUILayout.Height(28));
                    GUILayout.Space(10);
                    if (ColorButton("Create New Level", new Color(0.20f, 0.52f, 0.26f), 36)) CreateNew();
                    GUILayout.Space(5);
                    if (ColorButton("Load Level", new Color(0.20f, 0.36f, 0.60f), 36)) LoadMenu();
                }

                GUILayout.FlexibleSpace();
            }

            GUILayout.FlexibleSpace();
        }

        private void DrawLevelLoaded()
        {
            SP(8);
            DrawSaveBar();
            DrawSep();
            DrawSettings();
            DrawSep();
            DrawBrushBar();
            DrawSep();
            DrawUpperCanvas();
            SP(4);
            DrawLowerCanvas();
            DrawSep();
            DrawFooter();
            SP(8);
        }

        private void DrawSaveBar()
        {
            using (new GUILayout.HorizontalScope())
            {
                if (ColorButton("Save", new Color(0.25f, 0.55f, 0.25f), 24, 64)) Save();
                GUILayout.Space(6);
                GUILayout.Label(_level.name, EditorStyles.boldLabel);
                GUILayout.Label($"  ID {_level.levelID}", EditorStyles.miniLabel);
            }
        }

        private void DrawSettings()
        {
            GUILayout.Label("Grid Settings", StyleSection);
            SP(3);
            EditorGUI.BeginChangeCheck();
            RowField("Upper  W", ref _level.collectableGridWidth,
                "H", ref _level.collectableGridHeight,
                "Init", new Color(0.22f, 0.46f, 0.78f), InitCollectable);
            RowField("Lower  W", ref _level.boxGridWidth,
                "H", ref _level.boxGridHeight,
                "Init", new Color(0.68f, 0.42f, 0.13f), InitBox);
            using (new GUILayout.HorizontalScope())
            {
                GUILayout.Label("Bench cap.", GUILayout.Width(60));
                _level.benchCapacity = EditorGUILayout.IntField(_level.benchCapacity, GUILayout.Width(30));
            }

            if (EditorGUI.EndChangeCheck()) MarkDirty();
        }

        private void RowField(string wLabel, ref int wVal, string hLabel, ref int hVal,
            string btnLabel, Color btnColor, System.Action onInit)
        {
            using (new GUILayout.HorizontalScope())
            {
                GUILayout.Label(wLabel, GUILayout.Width(58));
                wVal = EditorGUILayout.IntField(wVal, GUILayout.Width(28));
                GUILayout.Label(hLabel, GUILayout.Width(10));
                hVal = EditorGUILayout.IntField(hVal, GUILayout.Width(28));
                GUILayout.Space(4);
                if (ColorButton(btnLabel, btnColor, 17, 32)) onInit();
            }
        }

        private void DrawBrushBar()
        {
            GUILayout.Label("Brush", StyleSection);
            SP(2);
            using (new GUILayout.HorizontalScope())
            {
                BrushButton("Collectable", EditorBrushType.Collectable, BrushOn_C);
                GUILayout.Space(2);
                BrushButton("Box", EditorBrushType.Box, BrushOn_B);
                GUILayout.Space(2);
                BrushButton("Clear", EditorBrushType.Clear, BrushOn_X);
            }

            if (_brush != EditorBrushType.Clear)
            {
                SP(3);
                using (new GUILayout.HorizontalScope())
                {
                    GUILayout.Label("Team", GUILayout.Width(36));
                    DrawTeamDropdown();
                }
            }

            if (_brush == EditorBrushType.Box)
            {
                using (new GUILayout.HorizontalScope())
                {
                    GUILayout.Label("Capacity", GUILayout.Width(58));
                    _boxCap = EditorGUILayout.IntField(_boxCap, GUILayout.Width(28));
                    GUILayout.Label("  0 = default", EditorStyles.miniLabel);
                }
            }
        }

        private void BrushButton(string label, EditorBrushType type, Color activeColor)
        {
            var bg = GUI.backgroundColor;
            GUI.backgroundColor = _brush == type ? activeColor : BrushOff;
            if (GUILayout.Button(label, EditorStyles.miniButton, GUILayout.Width(80), GUILayout.Height(20)))
                _brush = type;
            GUI.backgroundColor = bg;
        }

        private void DrawTeamDropdown()
        {
            EnsureTeamDb();
            if (_teamDb == null || _teamDb.Count == 0)
            {
                GUILayout.Label("No TeamDatabase found.", EditorStyles.miniLabel);
                return;
            }

            var names = _teamDb.teams.Select(t => $"{t?.EditorSymbol} {t?.name}").ToArray();
            _teamIndex = Mathf.Clamp(_teamIndex, 0, _teamDb.Count - 1);
            _teamIndex = EditorGUILayout.Popup(_teamIndex, names, GUILayout.Width(140));
        }

        private Team ActiveTeam => _teamDb?.Get(_teamIndex);
        
        private void DrawUpperCanvas()
        {
            GUILayout.Label("Collectable + Conveyor", StyleSection);
            SP(2);

            if (_level.collectableCells == null || _level.collectableCells.Count == 0)
            {
                EditorGUILayout.HelpBox("Collectable grid not initialized.", MessageType.Info);
                return;
            }

            int W = _level.collectableGridWidth;
            int H = _level.collectableGridHeight;
            int totalCols = W + 2;
            int totalRows = H + 2;
            float cs = EditorConstants.CellSize;

            Rect canvas = GUILayoutUtility.GetRect(totalCols * cs, totalRows * cs);
            EditorGUI.DrawRect(canvas, BgCanvasUpper);

            Event e = Event.current;
            bool dirty = false;

            for (int cy = 0; cy < totalRows; cy++)
            {
                for (int cx = 0; cx < totalCols; cx++)
                {
                    Rect cr = new(canvas.x + cx * cs, canvas.y + cy * cs, cs - 1, cs - 1);

                    bool outerRing = cx == 0 || cx == totalCols - 1 || cy == 0 || cy == totalRows - 1;
                    if (outerRing)
                    {
                        DrawConveyorCell(cr, cx, cy, W, H);
                        continue;
                    }

                    int gx = cx - 1;
                    int gy = (H - 1) - (cy - 1);
                    var cell = _level.GetCollectableCell(gx, gy);
                    if (cell == null) continue;

                    Color bg = cell.isFilled && cell.team != null ? cell.team.EditorColor : CellEmpty;
                    EditorGUI.DrawRect(cr, bg);
                    DrawOutline(cr);
                    if (cell.isFilled)
                        GUI.Label(cr, cell.team?.EditorSymbol ?? "?", StyleCenter);

                    if (!ShouldPaint(cr, e, out int btn)) continue;

                    if (btn == 0)
                    {
                        HandleUpperClick(cell);
                        dirty = true;
                        e.Use();
                    }
                    else if (btn == 1)
                    {
                        UpperContextMenu(cell);
                        e.Use();
                    }
                    else if (btn == 2)
                    {
                        cell.Clear();
                        dirty = true;
                        e.Use();
                    }
                }
            }

            if (dirty)
            {
                MarkDirty();
                Repaint();
            }
        }

        private void DrawConveyorCell(Rect cr, int cx, int cy, int W, int H)
        {
            GetConveyorCellInfo(cx, cy, W, H, out Color bg, out string label);
            EditorGUI.DrawRect(cr, bg);
            DrawOutline(cr);
            GUI.Label(cr, label, StyleSmall);
        }

        private static void GetConveyorCellInfo(int cx, int cy, int W, int H, out Color bg, out string label)
        {
            int totalCols = W + 2;
            int totalRows = H + 2;
            bool isCorner = (cx == 0 || cx == totalCols - 1) && (cy == 0 || cy == totalRows - 1);

            if (isCorner)
            {
                bg = CellConvCorner;
                label = GetCornerArrow(cx, cy, W, H);
                return;
            }

            int waypointIndex = -1;

            if (cy == totalRows - 1 && cx > 0 && cx < totalCols - 1)
                waypointIndex = W - 1 - (cx - 1);
            else if (cx == 0 && cy > 0 && cy < totalRows - 1)
                waypointIndex = W + 1 + ((H - 1) - (cy - 1));
            else if (cy == 0 && cx > 0 && cx < totalCols - 1)
                waypointIndex = W + H + 2 + (cx - 1);
            else if (cx == totalCols - 1 && cy > 0 && cy < totalRows - 1)
                waypointIndex = 2 * W + H + 3 + (H - 1 - ((H - 1) - (cy - 1)));

            if (waypointIndex == 0)
            {
                bg = CellConvStart;
                label = "S";
            }
            else if (waypointIndex >= 0)
            {
                bg = CellConvAligned;
                label = (waypointIndex + 1).ToString();
            }
            else
            {
                bg = CellConveyor;
                label = "·";
            }
        }

        private static string GetCornerArrow(int cx, int cy, int W, int H)
        {
            int tc = W + 2, tr = H + 2;
            if (cx == 0 && cy == 0) return "↘";
            if (cx == tc - 1 && cy == 0) return "↙";
            if (cx == tc - 1 && cy == tr - 1) return "↖";
            if (cx == 0 && cy == tr - 1) return "↗";
            return "·";
        }

        private void HandleUpperClick(CollectableGridCellData cell)
        {
            if (_brush == EditorBrushType.Collectable && ActiveTeam != null)
                cell.SetCollectable(ActiveTeam);
            else if (_brush == EditorBrushType.Clear)
                cell.Clear();
        }

        private void UpperContextMenu(CollectableGridCellData cell)
        {
            if (!cell.isFilled) return;
            EnsureTeamDb();
            var menu = new GenericMenu();
            if (_teamDb != null)
                foreach (var t in _teamDb.teams)
                {
                    var cap = t;
                    menu.AddItem(new GUIContent($"Team/{t.name}"), cell.team == t, () =>
                    {
                        cell.SetCollectable(cap);
                        MarkDirty();
                        Repaint();
                    });
                }

            menu.AddSeparator("");
            menu.AddItem(new GUIContent("Clear"), false, () =>
            {
                cell.Clear();
                MarkDirty();
                Repaint();
            });
            menu.ShowAsContext();
        }
        
        private void DrawLowerCanvas()
        {
            GUILayout.Label("Box Spawns", StyleSection);
            SP(2);

            if (_level.boxCells == null || _level.boxCells.Count == 0)
            {
                EditorGUILayout.HelpBox("Box grid not initialized.", MessageType.Info);
                return;
            }

            int W = _level.boxGridWidth;
            int H = _level.boxGridHeight;
            float cs = EditorConstants.CellSize;

            Rect canvas = GUILayoutUtility.GetRect(W * cs, H * cs);
            EditorGUI.DrawRect(canvas, BgCanvasLower);

            Event e = Event.current;
            bool dirty = false;

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
                    if (cell.isFilled)
                    {
                        string lbl = (cell.team?.EditorSymbol ?? "?")
                                     + (cell.capacityOverride > 0 ? $"\n{cell.capacityOverride}" : "");
                        GUI.Label(cr, lbl, StyleCenter);
                    }

                    if (!ShouldPaint(cr, e, out int btn)) continue;

                    if (btn == 0)
                    {
                        HandleBoxClick(cell);
                        dirty = true;
                        e.Use();
                    }
                    else if (btn == 1)
                    {
                        BoxContextMenu(cell);
                        e.Use();
                    }
                    else if (btn == 2)
                    {
                        cell.Clear();
                        dirty = true;
                        e.Use();
                    }
                }
            }

            if (dirty)
            {
                MarkDirty();
                Repaint();
            }
        }

        private void HandleBoxClick(BoxGridCellData cell)
        {
            if (_brush == EditorBrushType.Box && ActiveTeam != null)
                cell.SetBox(ActiveTeam, _boxCap);
            else if (_brush == EditorBrushType.Clear)
                cell.Clear();
        }

        private void BoxContextMenu(BoxGridCellData cell)
        {
            if (!cell.isFilled) return;
            EnsureTeamDb();
            var menu = new GenericMenu();
            if (_teamDb != null)
                foreach (var t in _teamDb.teams)
                {
                    var cap = t;
                    menu.AddItem(new GUIContent($"Team/{t.name}"), cell.team == t, () =>
                    {
                        cell.SetBox(cap, cell.capacityOverride);
                        MarkDirty();
                        Repaint();
                    });
                }

            menu.AddSeparator("");
            menu.AddItem(new GUIContent("Clear"), false, () =>
            {
                cell.Clear();
                MarkDirty();
                Repaint();
            });
            menu.ShowAsContext();
        }
        
        private void DrawFooter()
        {
            using (new GUILayout.HorizontalScope())
            {
                if (ColorButton("Validate", new Color(0.25f, 0.65f, 0.25f), 22, 80))
                    RunValidation();
                GUILayout.Space(3);
                if (ColorButton("Preview", new Color(0.20f, 0.50f, 0.65f), 22, 70))
                    LevelPreviewWindow.Show(_level);
                GUILayout.Space(3);
                if (ColorButton("Clear All", new Color(0.65f, 0.25f, 0.25f), 22, 76))
                {
                    if (EditorUtility.DisplayDialog("Clear All", "Clear all cells in both grids?", "Yes", "Cancel"))
                    {
                        foreach (var c in _level.collectableCells) c.Clear();
                        foreach (var c in _level.boxCells) c.Clear();
                        MarkDirty();
                        Repaint();
                    }
                }

                GUILayout.FlexibleSpace();
                int n = _level.collectableGridWidth;
                GUILayout.Label($"Waypoints: {4 * n + 4}", EditorStyles.miniLabel);
            }
        }
        
        private void CreateNew()
        {
            if (!Directory.Exists(EditorConstants.LevelsFolder))
                Directory.CreateDirectory(EditorConstants.LevelsFolder);
            int nextID = AssetDatabase.FindAssets("t:LevelData", new[] { EditorConstants.LevelsFolder }).Length + 1;
            string path = Path.Combine(EditorConstants.LevelsFolder, $"Level_{nextID:D3}.asset");
            var level = CreateInstance<LevelData>();
            level.levelID = nextID;
            AssetDatabase.CreateAsset(level, path);
            AssetDatabase.SaveAssets();
            _level = level;
            _state = WindowState.LevelLoaded;
            EnsureTeamDb();
            Repaint();
        }

        private void LoadMenu()
        {
            var guids = AssetDatabase.FindAssets("t:LevelData", new[] { EditorConstants.LevelsFolder });
            if (guids.Length == 0)
            {
                EditorUtility.DisplayDialog("No Levels", $"No LevelData found in:\n{EditorConstants.LevelsFolder}",
                    "OK");
                return;
            }

            var menu = new GenericMenu();
            foreach (var guid in guids)
            {
                var l = AssetDatabase.LoadAssetAtPath<LevelData>(AssetDatabase.GUIDToAssetPath(guid));
                if (l == null) continue;
                menu.AddItem(new GUIContent(l.name), false, () =>
                {
                    _level = l;
                    _state = WindowState.LevelLoaded;
                    EnsureTeamDb();
                    Repaint();
                });
            }

            menu.ShowAsContext();
        }

        private void Save()
        {
            if (!_level) return;
            EditorUtility.SetDirty(_level);
            AssetDatabase.SaveAssets();
            ShowNotification(new GUIContent($"Saved  {_level.name}"));
        }

        private void InitCollectable()
        {
            _level.InitCollectableGrid();
            MarkDirty();
            AssetDatabase.SaveAssets();
            Repaint();
        }

        private void InitBox()
        {
            _level.InitBoxGrid();
            MarkDirty();
            Repaint();
        }

        private void RunValidation()
        {
            if (!_level) return;
            var result = LevelValidator.Validate(_level);
            string body = result.IsValid
                ? $"Level is valid.{(result.Warnings.Count > 0 ? $"\n\n{result.Warnings.Count} warning(s):\n" + string.Join("\n", result.Warnings) : "")}"
                : $"{result.Errors.Count} error(s):\n\n" + string.Join("\n", result.Errors)
                                                         + (result.Warnings.Count > 0
                                                             ? $"\n\n{result.Warnings.Count} warning(s):\n" +
                                                               string.Join("\n", result.Warnings)
                                                             : "");
            EditorUtility.DisplayDialog(result.IsValid ? "Valid" : "Invalid", body, "OK");
        }


        private void EnsureTeamDb()
        {
            if (_teamDb != null) return;
            _teamDb = AssetDatabase.LoadAssetAtPath<TeamDatabase>(EditorConstants.TeamDatabasePath);
            if (_teamDb == null)
            {
                var guids = AssetDatabase.FindAssets("t:TeamDatabase");
                if (guids.Length > 0)
                    _teamDb = AssetDatabase.LoadAssetAtPath<TeamDatabase>(AssetDatabase.GUIDToAssetPath(guids[0]));
            }
        }

        private void MarkDirty()
        {
            if (_level) EditorUtility.SetDirty(_level);
        }

        private static void DrawOutline(Rect r)
        {
            Handles.BeginGUI();
            Handles.DrawSolidRectangleWithOutline(r, Color.clear, Outline);
            Handles.EndGUI();
        }

        private static void DrawSep()
        {
            GUILayout.Space(4);
            EditorGUI.DrawRect(GUILayoutUtility.GetRect(1, 1), SepColor);
            GUILayout.Space(4);
        }

        private static bool ColorButton(string label, Color color, float h, float w = -1)
        {
            var bg = GUI.backgroundColor;
            GUI.backgroundColor = color;
            bool result = w > 0
                ? GUILayout.Button(label, GUILayout.Height(h), GUILayout.Width(w))
                : GUILayout.Button(label, GUILayout.Height(h));
            GUI.backgroundColor = bg;
            return result;
        }

        private static void SP(float px) => GUILayout.Space(px);
        private void OnEnable() => EnsureTeamDb();
    }
}
#endif