using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Assets.Script.TweenLibrary.Effect
{
    public class ColorEffect : MUI_Effect
    {
        public event Action<IUI_Effect> _OnCompleted;
        YieldInstruction _wait { get; }
        [SerializeField] float _changeSpeed { get; }
        [SerializeField]private Color _baseColor { get; } // 기본 컬러색상
        [SerializeField] private Color _onPointerColor { get; } // 커서 다가갔을떄
        [SerializeField] private Color _onHighlightColor { get; } // 눌리거나 데미지를 받거나할떄
        [SerializeField] private Image _image { get; }  // 기본 이미지

        public ColorEffect( Image image,Color baseColor, Color hightLightColor, 
           YieldInstruction wait = null , float speed = 10f, Action<IUI_Effect> nextFunc=null)
        {
            _baseColor = baseColor; _onHighlightColor = hightLightColor; _image = image;
            _changeSpeed = speed;  _wait = wait; _OnCompleted += nextFunc;
        }
        public ColorEffect(Image image, Color baseColor,Color pointerColor ,Color hightLightColor, 
         YieldInstruction wait = null, float speed = 10f, Action<IUI_Effect> nextFunc = null)
        {
            _onPointerColor = pointerColor; _baseColor = baseColor; _onHighlightColor = hightLightColor;
            _changeSpeed = speed; _image = image; _wait = wait; _OnCompleted += nextFunc;
        }
        public override IEnumerator Execute()
        {
            base.Execute();
            _image.raycastTarget = false;
            float time = 0;

            while (_image.color != _onHighlightColor)
            {
                time += Time.deltaTime * _changeSpeed;
                _image.color = Color.Lerp(_baseColor, _onHighlightColor,time);

                yield return null;
            }
            time = 0f;
            yield return _wait;

            while (_image.color != _baseColor)
            {
                time += Time.deltaTime * _changeSpeed;
                _image.color = Color.Lerp(_onHighlightColor, _baseColor, time);

                yield return null;
            }
            
            _OnCompleted?.Invoke(this);
            _image.raycastTarget = true;
        }
    }
}
