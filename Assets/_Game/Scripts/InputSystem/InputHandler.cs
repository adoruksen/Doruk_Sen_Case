using UnityEngine;
using UnityEngine.EventSystems;

namespace RubyCase.Core
{
    public class InputHandler : MonoBehaviour
    {
        [SerializeField] private LayerMask clickableLayer;
        private float _timer = .25f;
        private float _cooldown;

        private Camera _camera;

        private void Awake() => _camera = Camera.main;

        private void Update()
        {
            _cooldown += Time.deltaTime;
            if(_cooldown < _timer) return;
            if (!Input.GetMouseButtonDown(0)) return;
            if (EventSystem.current != null && EventSystem.current.IsPointerOverGameObject()) return;

            var ray = _camera.ScreenPointToRay(Input.mousePosition);
            if (!Physics.Raycast(ray, out var hit, Mathf.Infinity, clickableLayer)) return;

            var clickable = hit.collider.GetComponentInParent<IClickable>();
            if (clickable is { IsClickable: true }) 
            {
                _cooldown = 0;
                clickable.OnClicked();
            }
        }
    }
}