using System;
using System.Collections;
using IIMLib.Core;
using IIMLib.Loop.Message;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace IIMLib.Loop
{
    public class LoadingScreenHandler : MonoBehaviour, ILoadingScreenHandler
    {
        [field: SerializeField] public Canvas LoadingCanvas { get; private set; }

        [field: SerializeField] public Slider LoadingBar { get; private set; }

        [field: SerializeField] public TMP_Text LoadingText { get; private set; }

        [field: SerializeField] public TMP_Text TooltipText { get; private set; }

        public bool IsLoading { get; private set; }

        public void InitializeLoadingScreen(Func<IEnumerator> loadingFunc, Delegate finished = null)
        {
            if (IsLoading) return;

            IsLoading = true;
            LoadingCanvas.gameObject.SetActive(true);

            StartCoroutine(ProcessLoadingScreenFunction(loadingFunc, null, 0, finished));
        }

        public void InitializeLoadingScreen(Func<IEnumerator> loadingFunc, in string[] tooltips, in float time = 3, Delegate finished = null)
        {
            if (IsLoading) return;

            IsLoading = true;
            LoadingCanvas.gameObject.SetActive(true);

            StartCoroutine(ProcessLoadingScreenFunction(loadingFunc, tooltips, time, finished));
        }

        public IEnumerator ProcessLoadingScreenFunction(Func<IEnumerator> loadingFunc, string[] tooltips,
            float time = 0, Delegate finished = null)
        {
            var loadingTextCoroutine = StartCoroutine(((ILoadingScreenHandler) this).UpdateLoadingText());
            var loadingTooltipTextCoroutine =
                StartCoroutine(((ILoadingScreenHandler) this).UpdateLoadingTooltipText(tooltips, time));

            yield return StartCoroutine(TrackProgress(loadingFunc()));

            IsLoading = false;

            StopCoroutine(loadingTextCoroutine);
            StopCoroutine(loadingTooltipTextCoroutine);
            LoadingCanvas.gameObject.SetActive(false);
            finished?.DynamicInvoke();
            ServiceLocator.Get<IMessageService>().Publish(new OnLoadFinishedMessage());
        }

        public IEnumerator TrackProgress(IEnumerator loadingFunc)
        {
            while (loadingFunc.MoveNext())
            {
                LoadingBar.value = loadingFunc.Current switch
                {
                    float currentProgressSingle => Mathf.Clamp((int)currentProgressSingle, 0,100),
                    int currentProgressInt => Mathf.Clamp(currentProgressInt, 0, 100),
                    _ => LoadingBar.value
                };
            
                yield return null;
            }
        }
    }
}
