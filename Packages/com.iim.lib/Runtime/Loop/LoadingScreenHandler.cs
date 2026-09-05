using System;
using System.Collections;
using IIMLib.Core.Message;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace IIMLib.Loop
{
    public sealed class LoadingScreenHandler : MonoBehaviour, ILoadingScreenHandler
    {
        [SerializeField] private Canvas _loadingCanvas;
        [SerializeField] private Slider _loadingSlider;
        [SerializeField] private TMP_Text _loadingText;
        [SerializeField] private TMP_Text _tooltipText;

        public bool IsLoading { get; private set; }
        public Canvas LoadingCanvas => _loadingCanvas;
        public Slider LoadingSlider => _loadingSlider;
        public TMP_Text LoadingText => _loadingText;
        public TMP_Text TooltipText => _tooltipText;

        public void Initialize(
            Func<IEnumerator> loadingFunc,
            Action finished = null,
            float loadingTextInterval = 0.5f,
            float tooltipInterval = 4f,
            params string[] tooltips)
        {
            if (loadingFunc == null)
                throw new ArgumentNullException(nameof(loadingFunc));

            StopAllCoroutines();
            StartCoroutine(Process(loadingFunc, finished, loadingTextInterval, tooltipInterval, tooltips));
        }

        private IEnumerator Process(
            Func<IEnumerator> loadingFunc,
            Action finished,
            float loadingTextInterval,
            float tooltipInterval,
            string[] tooltips)
        {
            IsLoading = true;

            if (_loadingCanvas != null)
                _loadingCanvas.gameObject.SetActive(true);

            if (_loadingSlider != null)
                _loadingSlider.value = 0f;

            StartCoroutine(((ILoadingScreenHandler)this).UpdateLoadingText(loadingTextInterval));
            StartCoroutine(((ILoadingScreenHandler)this).UpdateLoadingTooltipText(tooltipInterval, tooltips));

            yield return TrackProgress(loadingFunc());

            IsLoading = false;

            if (_loadingSlider != null)
                _loadingSlider.value = 1f;

            if (_loadingCanvas != null)
                _loadingCanvas.gameObject.SetActive(false);

            finished?.Invoke();

            if (IIMLib.Core.ServiceLocator.TryGet<IMessageService>(out var messageService))
                messageService.Publish(new Message.OnLoadFinishedMessage());
        }

        private IEnumerator TrackProgress(IEnumerator loading)
        {
            if (loading == null)
                yield break;

            while (loading.MoveNext())
            {
                switch (loading.Current)
                {
                    case float currentProgressSingle:
                        if (_loadingSlider != null)
                            _loadingSlider.value = Mathf.Clamp01(currentProgressSingle);
                        break;

                    case double currentProgressDouble:
                        if (_loadingSlider != null)
                            _loadingSlider.value = Mathf.Clamp01((float)currentProgressDouble);
                        break;
                }

                yield return loading.Current;
            }
        }
    }
}
