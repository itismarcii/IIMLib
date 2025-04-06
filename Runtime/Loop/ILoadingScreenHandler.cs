using System;
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
        private const string LOADING_DEFAULT_TEXT = "LOADING";
        
        public Canvas LoadingCanvas { get; }
        public Slider LoadingBar { get; }
        public TMP_Text LoadingText { get; }
        public TMP_Text TooltipText { get; }
        public bool IsLoading { get; }
        
        public void InitializeLoadingScreen(Func<IEnumerator> loadingFunc, Delegate finished = null);

        public void InitializeLoadingScreen(Func<IEnumerator> loadingFunc, in string[] tooltips, in float time = 3f,
            Delegate finished = null);

        public IEnumerator ProcessLoadingScreenFunction(Func<IEnumerator> loadingFunc, string[] tooltips,
            float time = 0, Delegate finished = null);
        
        public IEnumerator TrackProgress(IEnumerator loadingFunc);
       
        public IEnumerator UpdateLoadingText()
        {
            var wait = new WaitForSeconds(1f);
            var dotCount = 0;
        
            while (IsLoading)
            {
                var dotString = LOADING_DEFAULT_TEXT;

                for (var i = 0; i < dotCount; i++)
                {
                    dotString += ".";
                }

                LoadingText.text = dotString;
                dotCount++;
                yield return wait;

                if (dotCount == 4)
                {
                    dotCount = 0;
                }
            }

            LoadingText.text = "";
        }    
        
        public IEnumerator UpdateLoadingTooltipText(string[] tooltips, float time)
        {
            if(time <= 0) yield break;
        
            var wait = new WaitForSeconds(time);
            var tooltipQueue = new Queue<string>(HelperCollection.Shuffle(tooltips));

            while (IsLoading)
            {
                TooltipText.text = tooltipQueue.Dequeue();
                yield return wait;
            }
        }
    }
}