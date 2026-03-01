using RubyCase.Gameplay.BoxSystem;
using UnityEngine;

namespace RubyCase.Gameplay.BenchSystem
{
    public class BenchController : MonoBehaviour
    {
        public bool IsAvailable => !IsReserved && !IsOccupied;
        public bool IsReserved { get; private set; }
        public bool IsOccupied { get; private set; }

        public BoxController CurrentBox { get; private set; }

        public void Reserve(BoxController box)
        {
            CurrentBox = box;
            IsReserved = true;
            IsOccupied = false;
        }

        public void Occupy()
        {
            IsReserved = false;
            IsOccupied = true;
        }

        public void Release()
        {
            CurrentBox = null;
            IsReserved = false;
            IsOccupied = false;
        }
    }
}