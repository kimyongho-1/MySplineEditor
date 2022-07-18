using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
namespace Assets.Script.TweenLibrary.Effect
{
    public class SnapEffect : IUI_Effect
    {
        public event Action<IUI_Effect> _OnCompleted;
        RectTransform _rectTransform { get; }

        Vector2 _snapToSize;

        public SnapEffect(RectTransform rt, Vector2 snapSize)
        {
            _rectTransform = rt; _snapToSize = snapSize; 
        }
        public void UpdateSnapSize(Vector2 snapSize)
        {
            _snapToSize = snapSize;
        }

        public IEnumerator Execute()
        {
            Vector2 origin = _rectTransform.sizeDelta;
            _rectTransform.sizeDelta = _snapToSize;
            Debug.Log("Origin : " + origin + " snapSize : " + _snapToSize);
            UpdateSnapSize(origin);
            
            yield break;
        }
    }
}
