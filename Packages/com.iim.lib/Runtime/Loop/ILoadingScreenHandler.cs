using System.Collections;
using System.Collections.Generic;
using IIMLib.Core;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace IIMLib.Loop
{
    public interface ILoadingScreenHandler
    {
        bool IsLoading { get; }

        Canvas LoadingCanvas { get; }
        Slider LoadingSlider { get; }
        TMP_Text LoadingText { get; }
        TMP_Text TooltipText { get; }

        IEnumerator UpdateLoadingText(float time)
        {
            if (LoadingText == null || time <= 0f)
                yield break;

            var dotCount = 0;

            while (IsLoading)
            {
                LoadingText.text = $"LOADING{new string('.', dotCount)}";
                dotCount = (dotCount + 1) % 4;
                yield return new WaitForSecondsRealtime(time);
            }

            LoadingText.text = string.Empty;
        }

        IEnumerator UpdateLoadingTooltipText(float time, params string[] tooltips)
        {
            if (TooltipText == null || time <= 0f || tooltips == null || tooltips.Length == 0)
                yield break;

            var shuffled = new List<string>(HelperCollection.Shuffle(tooltips));
            var index = 0;

            while (IsLoading)
            {
                TooltipText.text = shuffled[index];
                index = (index + 1) % shuffled.Count;
                yield return new WaitForSecondsRealtime(time);
            }

            TooltipText.text = string.Empty;
        }
    }
}
