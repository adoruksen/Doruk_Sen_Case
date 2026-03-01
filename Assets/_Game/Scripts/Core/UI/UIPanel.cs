using Cysharp.Threading.Tasks;
using DG.Tweening;
using UnityEngine;

namespace RubyCase.Core.UI
{
    [RequireComponent(typeof(CanvasGroup))]
    public class UIPanel : MonoBehaviour
    {
        private CanvasGroup _group;
        private Tween _tween;

        protected virtual void Awake()
        {
            _group = GetComponent<CanvasGroup>();
            ForceHide();
        }

        public async UniTask ShowAsync(float duration)
        {
            _tween?.Kill();
            _group.alpha = 0f;
            _group.interactable = false;
            _group.blocksRaycasts = false;

            _tween = _group.DOFade(1f, duration).SetEase(Ease.OutQuad);
            await _tween.AsyncWaitForCompletion();

            _group.interactable = true;
            _group.blocksRaycasts = true;
        }

        public async UniTask HideAsync(float duration)
        {
            _tween?.Kill();
            _group.interactable = false;
            _group.blocksRaycasts = false;

            _tween = _group.DOFade(0f, duration).SetEase(Ease.InQuad);
            await _tween.AsyncWaitForCompletion();
        }

        public void ForceHide()
        {
            _tween?.Kill();
            _group.alpha = 0f;
            _group.interactable = false;
            _group.blocksRaycasts = false;
        }

        private void OnDestroy() => _tween?.Kill();
    }
}