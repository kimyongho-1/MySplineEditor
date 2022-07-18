using Assets.Script.TweenLibrary;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
[ExecuteAlways]
public class PathCreator : MonoBehaviour  // 컴포넌트 역할, 점(Paths.cs)의 정보를 꺼내 씀
{
    public Paths path;
    public Dictionary<int, Paths> allPaths = new Dictionary<int, Paths>();
    EffectBuilder effect;
    public Transform mover;
    [Header("이동속도")] public float MoveSpeed;
    [Header("삭제 민감도")] public float _deleteSensibilty;
    [Header("시작점")] public Color startAnchor;
    [Header("끝점")] public Color endAnchor;
    [Header("중심점")] public Color anchor;
    [Header("핸들")] public Color handle;
    [Header("점 굵기"), Range(1, 16)] public int pointthickness = 6;
    [Header("베지어 선")] public Color bezier;
    [Header("베지어선 굵기"), Range(1, 10)] public float thickness;
    bool loop;
    int count, NumSeg = 0;
    int currIdx;
    
    public int CurrPathIdx { get { return currIdx; } set { currIdx = value; } }
    private void OnValidate()
    {
        if (allPaths.Count == 0) { return; }
        Debug.Log(allPaths.Count);
        CurrPathIdx =  0;
    }
    private void Start()
    {
        effect = new EffectBuilder(this);
        CurrPathIdx = 0;
        startAnchor.a = endAnchor.a = anchor.a = handle.a = bezier.a = 1; // 투명도1로 고정
        _deleteSensibilty = (path.points[0].position - path.points[1].position).magnitude;
        loop = true;
    }

    public void CreatePath() // 패스나 커브를 사용할 오브젝트는 Init함수에서 이 함수를 실행시키면됨 (Utill.GetOrAddComponent())
    {
        allPaths.Clear();
        path = new Paths(transform.position, 10f);
        allPaths.Add(allPaths.Count, path);
        //allPaths.Add(0,path);
    }

    public void AddNewPath(Vector2 mousePos) // 여러개의 패스를 사용할수있도록
    {
        allPaths.Add(allPaths.Count, new Paths(mousePos));
    }
    public void ChangeControlPathIDX(int idx) // 딕셔너리 키값에 각각 매핑된 개별 패스들중 인자 키번호껄로 접근
    {
        path = allPaths[idx]; // 이제 선추가 점 생성등등 컨트롤을 idx번호 패스객체에서 하기로함
    }

    public void DoMove()
    {
        StopAllCoroutines();
        StartCoroutine(Move());
    }
    public void CancelMove()
    {
        StopAllCoroutines();
        mover.position = new Vector3(path.points[0].position.x, path.points[0].position.y, 0);
    }

    int LoopIdx(int idx) // IsClosed == true, 즉 시작과 끝이 연결된 형태면 첫인덱스과 끝인덱스가 위치가 같아지기에 순환하게함
    {
        return (idx + path.points.Count) & path.points.Count;
    }
   
    IEnumerator Move()
    {
        count = 0;
        mover.position = new Vector3(path.points[0].position.x, path.points[0].position.y, 0);
        mover.localScale = Vector3.one * 7f;
        float t = 0;
        NumSeg = path.NumSegment;
        Vector2 AB, BC, CD, AB_BC, BC_CD = default;
        while ((loop == true) ? true : count < NumSeg)
        {
            Point[] p = path.GetPointsInSegment(count);

            while (t < 1)
            {
                AB = Vector2.Lerp(p[0].position, p[1].position, t);
                BC = Vector2.Lerp(p[1].position, p[2].position, t);
                CD = Vector2.Lerp(p[2].position, p[3].position, t);
                AB_BC = Vector2.Lerp(AB, BC, t);
                BC_CD = Vector2.Lerp(BC, CD, t);

                mover.position = Vector2.Lerp(AB_BC, BC_CD, t);
                t = Mathf.Clamp(t + Time.deltaTime * MoveSpeed * 0.1f, 0, 1f);
                yield return null;

            }
            // 재생할 이벤트함수가 있다면 실행!
            if (p[3].effect != null) { p[3].effect.Execute(); }

            // 현재 이동이 무한반복상태인지 체크
            count = (loop == true) ? (count + 1) % path.NumSegment : count + 1;
            // 시간 초기화 
            t = 0;
            NumSeg = path.NumSegment; //재생중간에, IsClosed 결과값 바꿀경우 대비해서 새로 초기화
        }

    }
}
