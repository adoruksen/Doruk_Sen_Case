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

        private readonly List<GameObject> _collectables = new();
        private readonly List<GameObject> _boxes = new();
        private readonly List<GameObject> _conveyorNodes = new();

        [ShowInInspector, ReadOnly, FoldoutGroup("Runtime"), ListDrawerSettings(Expanded = true, DraggableItems = false, HideAddButton = true, HideRemoveButton = true)]
        public List<GameObject> Collectables => _collectables;

        [ShowInInspector, ReadOnly, FoldoutGroup("Runtime"), ListDrawerSettings(Expanded = true, DraggableItems = false, HideAddButton = true, HideRemoveButton = true)]
        public List<GameObject> Boxes => _boxes;

        [ShowInInspector, ReadOnly, FoldoutGroup("Runtime"), ListDrawerSettings(Expanded = true, DraggableItems = false, HideAddButton = true, HideRemoveButton = true)]
        public List<GameObject> ConveyorNodes => _conveyorNodes;

        [ShowInInspector, ReadOnly, FoldoutGroup("Runtime")]
        public int CollectablesCount => _collectables.Count;

        [ShowInInspector, ReadOnly, FoldoutGroup("Runtime")]
        public int BoxesCount => _boxes.Count;

        [ShowInInspector, ReadOnly, FoldoutGroup("Runtime")]
        public int ConveyorNodesCount => _conveyorNodes.Count;

        public void Initialize(LevelData data)
        {
            Data = data;
            IsReady = false;

            CollectablesRoot = EnsureChild("Collectables");
            BoxesRoot = EnsureChild("Boxes");
            ConveyorNodesRoot = EnsureChild("ConveyorNodes");
        }

        public void RegisterCollectable(GameObject go) => _collectables.Add(go);
        public void RegisterBox(GameObject go) => _boxes.Add(go);
        public void RegisterConveyorNode(GameObject go) => _conveyorNodes.Add(go);
        public void MarkReady() => IsReady = true;

        public void Clear()
        {
            Data = null;
            IsReady = false;

            _collectables.Clear();
            _boxes.Clear();
            _conveyorNodes.Clear();

            CollectablesRoot = null;
            BoxesRoot = null;
            ConveyorNodesRoot = null;
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
