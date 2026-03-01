using System;
using UnityEngine;

namespace RubyCase.Gameplay.BoxSystem
{
    public class BoxSlot : MonoBehaviour
    {
        public bool IsAvailable => !IsReserved && !IsOccupied;
        public bool IsReserved { get; private set; }
        public bool IsOccupied { get; private set; }
        private GameObject _slotImage;

        public GameObject CurrentCollectable { get; private set; }

        private void Awake()
        {
            _slotImage = transform.GetChild(0).gameObject;
        }

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
            _slotImage.SetActive(false);
        }

        public void Release()
        {
            CurrentCollectable = null;
            IsReserved = false;
            IsOccupied = false;
            _slotImage.SetActive(true);
        }
    }
}