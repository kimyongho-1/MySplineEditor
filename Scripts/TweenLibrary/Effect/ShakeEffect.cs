using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Assets.Script.TweenLibrary.Effect
{
    public class ShakeEffect : IUI_Effect
    {
        public event Action<IUI_Effect> _OnCompleted;
        private RectTransform _rectTransform { get; }
        float MaxRotation { get; }
        float Speed { get; }
        float Delta { get; }
        YieldInstruction Wait { get; }

        public ShakeEffect(RectTransform rt, float maxRotation, float speed, 
            float delta = 0, Action<IUI_Effect> nextFunc = null)
        {
            _rectTransform = rt; MaxRotation = maxRotation; Speed = speed; 
            Delta = ((9.0f - delta) * 0.1f);
            _OnCompleted += nextFunc;
        }

        public IEnumerator Execute()
        {
            float currRot, earlyRot;
            currRot = earlyRot =_rectTransform.rotation.z;
            float nextRot = MaxRotation - 1F;
            float time = 0;

            while (Mathf.Abs(nextRot) > MaxRotation*0.1f)
            {
                time += Time.deltaTime * Speed;
                float newRot = Mathf.Lerp(currRot, nextRot, time);
                _rectTransform.rotation = Quaternion.Euler(0, 0, newRot);

                if (time >= 1)  // next회전량에 도달했으면, 새 회전목적지 갱신및 반대방향으로 회전하게끔 유도
                {
                    currRot = nextRot;
                    nextRot = (nextRot * Delta) * -1f; // -1f을 곱하는건 반대로도 회전하게 유도 + Delta값이 작아질수록 Wiggle빨리끝남
                    time = 0;
                }
                yield return null;
            }
            _rectTransform.rotation = Quaternion.Euler(new Vector3(0,0, earlyRot)); // 회전끝날시 첫 회전위치로 초기화

            _OnCompleted?.Invoke(this); // 추가 행동있으면 실행
        }
    }
}
