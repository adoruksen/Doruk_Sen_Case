using UnityEngine;

namespace RubyCase.BoxSystem
{
    public class BoxSlot : MonoBehaviour
    {
        public bool IsAvailable => !IsReserved && !IsOccupied;
        public bool IsReserved { get; private set; }
        public bool IsOccupied { get; private set; }

        public GameObject CurrentCollectable { get; private set; }

        public void Reserve(GameObject collectable)
        {
            CurrentCollectable = collectable;
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
            CurrentCollectable = null;
            IsReserved = false;
            IsOccupied = false;
        }
    }
}