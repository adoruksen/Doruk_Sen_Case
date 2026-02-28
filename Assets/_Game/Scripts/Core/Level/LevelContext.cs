using System.Collections.Generic;
using RubyCase.LevelSystem;
using Sirenix.OdinInspector;
using UnityEngine;

namespace RubyCase.Core
{
    public sealed class LevelContext : MonoBehaviour
    {
        [ShowInInspector, ReadOnly] public LevelData Data { get; private set; }
        [ShowInInspector, ReadOnly] public bool IsReady { get; private set; }
        public Vector3 CollectablesBottomLeft { get; set; }

        [ShowInInspector, ReadOnly, FoldoutGroup("Hierarchy")]
        public Transform CollectablesRoot { get; private set; }

        [ShowInInspector, ReadOnly, FoldoutGroup("Hierarchy")]
        public Transform BoxesRoot { get; private set; }

        [ShowInInspector, ReadOnly, FoldoutGroup("Hierarchy")]
        public Transform ConveyorRoot { get; private set; }

        [ShowInInspector, ReadOnly, FoldoutGroup("Hierarchy")]
        public Transform BenchesRoot { get; private set; }

        private readonly List<GameObject> _collectables = new();
        private readonly List<GameObject> _boxes = new();
        private readonly List<GameObject> _benches = new();

        [ShowInInspector, ReadOnly] public List<GameObject> Collectables => _collectables;
        [ShowInInspector, ReadOnly] public List<GameObject> Boxes => _boxes;
        [ShowInInspector, ReadOnly] public List<GameObject> Benches => _benches;

        [ShowInInspector, ReadOnly] public int CollectablesCount => _collectables.Count;
        [ShowInInspector, ReadOnly] public int BoxesCount => _boxes.Count;
        [ShowInInspector, ReadOnly] public int BenchesCount => _benches.Count;

        public void Initialize(LevelData data)
        {
            Data = data;
            IsReady = false;

            CollectablesRoot = EnsureChild("Collectables");
            BoxesRoot = EnsureChild("Boxes");
            ConveyorRoot = EnsureChild("Conveyor");
            BenchesRoot = EnsureChild("Benches");
        }

        public void RegisterCollectable(GameObject go) => _collectables.Add(go);
        public void RegisterBox(GameObject go) => _boxes.Add(go);
        public void RegisterBench(GameObject go) => _benches.Add(go);
        public void MarkReady() => IsReady = true;

        public void Clear()
        {
            Data = null;
            IsReady = false;
            CollectablesBottomLeft = default;

            _collectables.Clear();
            _boxes.Clear();
            _benches.Clear();

            CollectablesRoot = null;
            BoxesRoot = null;
            ConveyorRoot = null;
            BenchesRoot = null;
        }

        private Transform EnsureChild(string childName)
        {
            var existing = transform.Find(childName);
            if (existing != null) return existing;
            var go = new GameObject(childName);
            go.transform.SetParent(transform, false);
            return go.transform;
        }
    }
}