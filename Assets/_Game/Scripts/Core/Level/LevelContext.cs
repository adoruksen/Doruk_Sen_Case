using System.Collections.Generic;
using RubyCase.LevelSystem;
using Sirenix.OdinInspector;
using UnityEngine;

namespace RubyCase.Core
{
    public sealed class LevelContext : MonoBehaviour
    {
        [ShowInInspector, ReadOnly]
        public LevelData Data { get; private set; }

        [ShowInInspector, ReadOnly]
        public bool IsReady { get; private set; }

        [ShowInInspector, ReadOnly, FoldoutGroup("Hierarchy")]
        public Transform CollectablesRoot { get; private set; }

        [ShowInInspector, ReadOnly, FoldoutGroup("Hierarchy")]
        public Transform BoxesRoot { get; private set; }

        [ShowInInspector, ReadOnly, FoldoutGroup("Hierarchy")]
        public Transform ConveyorNodesRoot { get; private set; }

        [ShowInInspector, ReadOnly, FoldoutGroup("Hierarchy")]
        public Transform BenchesRoot { get; private set; }

        private readonly List<GameObject> _collectables = new();
        private readonly List<GameObject> _boxes = new();
        private readonly List<GameObject> _conveyorNodes = new();
        private readonly List<GameObject> _benches = new();

        [ShowInInspector, ReadOnly, FoldoutGroup("Runtime"), ListDrawerSettings(Expanded = true, DraggableItems = false, HideAddButton = true, HideRemoveButton = true)]
        public List<GameObject> Collectables => _collectables;

        [ShowInInspector, ReadOnly, FoldoutGroup("Runtime"), ListDrawerSettings(Expanded = true, DraggableItems = false, HideAddButton = true, HideRemoveButton = true)]
        public List<GameObject> Boxes => _boxes;

        [ShowInInspector, ReadOnly, FoldoutGroup("Runtime"), ListDrawerSettings(Expanded = true, DraggableItems = false, HideAddButton = true, HideRemoveButton = true)]
        public List<GameObject> ConveyorNodes => _conveyorNodes;

        [ShowInInspector, ReadOnly, FoldoutGroup("Runtime"), ListDrawerSettings(Expanded = true, DraggableItems = false, HideAddButton = true, HideRemoveButton = true)]
        public List<GameObject> Benches => _benches;

        [ShowInInspector, ReadOnly, FoldoutGroup("Runtime")]
        public int CollectablesCount => _collectables.Count;

        [ShowInInspector, ReadOnly, FoldoutGroup("Runtime")]
        public int BoxesCount => _boxes.Count;

        [ShowInInspector, ReadOnly, FoldoutGroup("Runtime")]
        public int ConveyorNodesCount => _conveyorNodes.Count;

        [ShowInInspector, ReadOnly, FoldoutGroup("Runtime")]
        public int BenchesCount => _benches.Count;

        public void Initialize(LevelData data)
        {
            Data = data;
            IsReady = false;

            CollectablesRoot = EnsureChild("Collectables");
            BoxesRoot = EnsureChild("Boxes");
            ConveyorNodesRoot = EnsureChild("ConveyorNodes");
            BenchesRoot = EnsureChild("Benches");
        }

        public void RegisterCollectable(GameObject go) => _collectables.Add(go);
        public void RegisterBox(GameObject go) => _boxes.Add(go);
        public void RegisterConveyorNode(GameObject go) => _conveyorNodes.Add(go);
        public void RegisterBench(GameObject go) => _benches.Add(go);
        public void MarkReady() => IsReady = true;

        public void Clear()
        {
            Data = null;
            IsReady = false;

            _collectables.Clear();
            _boxes.Clear();
            _conveyorNodes.Clear();
            _benches.Clear();

            CollectablesRoot = null;
            BoxesRoot = null;
            ConveyorNodesRoot = null;
            BenchesRoot = null;
        }

        private Transform EnsureChild(string name)
        {
            var t = transform.Find(name);
            if (t != null) return t;

            var go = new GameObject(name);
            go.transform.SetParent(transform, false);
            return go.transform;
        }
    }
}
