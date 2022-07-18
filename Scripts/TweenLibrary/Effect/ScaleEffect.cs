using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Assets.Script.TweenLibrary;
namespace Assets.Script.TweenLibrary.Effect
{
    public class ScaleEffect : IUI_Effect
    {
        public event Action<IUI_Effect> _OnCompleted;
        private RectTransform _rectTransform { get; }
        Vector3 MaxSize { get; }
        float Speed { get; }
        YieldInstruction Wait { get; }

        public ScaleEffect(RectTransform rt , Vector3 maxsize, float speed, 
            YieldInstruction wait , Action<IUI_Effect> nextFunc = null)
        {
            _rectTransform = rt; MaxSize = maxsize; Speed = speed; Wait = wait;
            _OnCompleted += nextFunc;
        }
    


        public IEnumerator Execute()
        {
            _rectTransform.GetComponent<Image>().raycastTarget = false; // 중복클릭 방지차원으로 레이꺼주기
            float time = 0f;
            Vector3 EarlyScale = _rectTransform.localScale; // 초기 스케일 캐싱

            while (_rectTransform.localScale != MaxSize) // 현재 크기에서 목표크기로 확장
            {
                time += Time.deltaTime * Speed;
                _rectTransform.localScale = Vector3.Lerp(EarlyScale, MaxSize, time);
                yield return null;
            }

            yield return Wait;

            Vector3 CurrentScale = _rectTransform.localScale; // 현재 크기에서 초기크기로 복귀
            time = 0f;

            while (_rectTransform.localScale != EarlyScale)
            {
                time += Time.deltaTime * Speed;
                _rectTransform.localScale = Vector3.Lerp(CurrentScale, EarlyScale, time);
                yield return null;
            }

            if (_OnCompleted != null)  // 다음 행위 준비되어있다면 실행
            { _OnCompleted.Invoke(this); }
            else
            { _rectTransform.GetComponent<Image>().raycastTarget = true; }  // 다시 누를수있게 이미지의 레이캐스트 활성화
        }

    
    }
}
