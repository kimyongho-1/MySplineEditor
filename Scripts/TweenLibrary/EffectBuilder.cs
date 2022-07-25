using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Assets.Script.TweenLibrary.Effect;
namespace Assets.Script.TweenLibrary
{
    public class EffectBuilder  // 각 UI클래스,오브젝트에서 이 클래스를 생성함 + 이 클래스에선 재생할 이펙트를 리스트에 보관 + 함수통해서 재생
    {
        MonoBehaviour Owner { get; } // 이펙트를 실행할, 이펙트 빌더를 사용할 오브젝트 (코루틴 쓰기위해서 모노비헤이버를 기준으로 정함 추후 바뀔지는 미지수)
        public List<IUI_Effect> _effectList = new List<IUI_Effect>();
        int count = 0; // 재생인덱스 순서가 있다면  현재 순서를 캐싱해두기 위한 임시멤버변수

        public EffectBuilder(MonoBehaviour ownerObj) // 생성자 : 인자는 이펙트를 실행해야할 게임오브젝트
        {
            Owner = ownerObj;
        }

        public EffectBuilder AddEffect(IUI_Effect ToAddEffect) // 이펙트를 이펙트빌더에 추가시키는 함수
        {   
            _effectList.Add(ToAddEffect);
            return this;   // 반환값을 자신으로 한건 추가함과 재생도 동시에 할수있게 유도 (아래 Execute()함수로 바로 실행가능하게)
        }
        public  void Execute() // 위에  AddEffect로 생성후 .Execute()로 마지막에 추가한 이펙트 바로 재생유도
        {
            ExecuteSelectedEffect(_effectList.Count - 1);
            #region 사용예시 In anyUIscript.cs
        //  BindEvent(Get<Button>((int)Buttons.HealthPanel).gameObject,
        //  _effect.AddEffect(new ScaleEffect(Get<Button>((int)Buttons.HealthPanel).gameObject.GetComponent<RectTransform>(), new Vector3(4f, 3f, 0), 3f, new WaitForSeconds(1.5f))).Execute
        // , Define.Mouse.Click); 
            #endregion
        }
        public void ExecuteAllEffects() // 저장해둔 이펙트 모두 실행 (순서없이 한꺼번에)
        {
            Owner.StopAllCoroutines();  // 기존에 실행중인게 있다면 꺼두기
            foreach (IUI_Effect eachEffect in _effectList)
            {
                Owner.StartCoroutine(eachEffect.Execute()); // 한번에 모두 실행
            }
        }
        public void ExecuteSelectedEffect(int idx, bool beforeStopAnim = false) // 한개만 실행 , 인자로 인덱스번호를받기 + 먼저 실행중인걸 취소할건지 여부
        {
            if (beforeStopAnim == true) { Owner.StopAllCoroutines(); }
            Owner.StartCoroutine(_effectList[idx].Execute());    
        }
        public void ExecuteProcedure(IUI_Effect nextFunc)  // 인덱스번호순으로 순차적 재생
        {
            if (count == _effectList.Count - 1) { count = 0; return; }
            count += 1;
            ExecuteSelectedEffect(count);
            #region 사용예시 In anyUIscript.cs
            // 1) Init함수내
            //    _effect.AddEffect(new ScaleEffect(Get<Button>((int)Buttons.HealthPanel).gameObject.GetComponent<RectTransform>(), new Vector3(4f, 3f, 0), 3f, new WaitForSeconds(1.5f), _effect.ExecuteProcedure));
            //    BindEvent(Get<Button>((int)Buttons.HealthPanel).gameObject, DoProcedureUIanim, Define.Mouse.Click);
            // 2) UI스크립트내 추가함수 생성
            //    void DoProcedureUIanim() { _effect.ExecuteSelectedEffect(0); }  언제나 0번인덱스순으로 재생한다면 인자 0으로
            
            //    또는

            // 1) _effect.AddEffect(new ScaleEffect(Get<Button>((int)Buttons.HealthPanel).gameObject.GetComponent<RectTransform>(), new Vector3(4f, 3f, 0), 3f, new WaitForSeconds(1.5f), _effect.ExecuteProcedure));
            //    BindEvent(Get<Button>((int)Buttons.HealthPanel).gameObject, _effect.ExecuteProcedure, Define.Mouse.Click);
            //    "아래 인자없는 함수 버전으로 순차적재생"
            #endregion
        }
        public void ExecuteProcedure() // 0번인덱스부터 재생되도록 유도하는 함수 "Warning : Init함수에서 이펙트추가시  Action<IUI_Effect> : _effect.ExecuteProcedure로 다음함수 실행되도록 작성 필수임!!"
        { ExecuteSelectedEffect(0);  }
    }
}
