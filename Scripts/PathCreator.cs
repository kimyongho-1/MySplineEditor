using System.Collections;
using System.Collections.Generic;
using UnityEngine;
[ExecuteAlways]
public class PathCreator : MonoBehaviour  // 컴포넌트 역할, 점(Paths.cs)의 정보를 꺼내 씀
{
    [HideInInspector]public Paths path;
    public Transform mover;
    [Header("이동속도")] public float MoveSpeed;
    [Header("삭제 민감도")]public float _deleteSensibilty;
    [Header("시작점")] public Color startAnchor;
    [Header("끝점")] public Color endAnchor;
    [Header("중심점")] public Color anchor;
    [Header("핸들")] public Color handle;
    [Header("점 굵기"), Range(1, 6)] public int pointthickness;
    [Header("베지어 선")] public Color bezier;
    [Header("베지어선 굵기"),Range(1,10)] public float thickness;
    bool loop;
    int count, NumSeg = 0;

    private void OnValidate()
    {
        _deleteSensibilty = (path.points[0] - path.points[1]).magnitude;
    }
    private void Start()
    {
        _deleteSensibilty = (path.points[0] - path.points[1]).magnitude;
        loop = true;
    }

    public void CreatePath()
    {
        path = new Paths(transform.position, 15f);
    }

    public void DoMove()
    {
        StopAllCoroutines();
        StartCoroutine(Move());
    }
    public void CancelMove()
    {
        StopAllCoroutines();
        mover.position = new Vector3(path.points[0].x, path.points[0].y, 0);
    }

    int LoopIdx(int idx) // IsClosed == true, 즉 시작과 끝이 연결된 형태면 첫인덱스과 끝인덱스가 위치가 같아지기에 순환하게함
    {
        return (idx + path.points.Count) & path.points.Count;
    }
 
    IEnumerator Move()
    {
        count = 0;
        mover.position = new Vector3(path.points[0].x, path.points[0].y,0);
        mover.localScale = Vector3.one * 7f;
        float t = 0;
        NumSeg = path.NumSegment;
        while ((loop == true) ? true : count < NumSeg) 
        {
            Vector2[] p = path.GetPointsInSegment(count);

            while (t < 1)
            {
                Vector2 AB = Vector2.Lerp(p[0], p[1], t);
                Vector2 BC = Vector2.Lerp(p[1], p[2], t);
                Vector2 CD = Vector2.Lerp(p[2], p[3], t);
                Vector2 AB_BC = Vector2.Lerp(AB, BC, t);
                Vector2 BC_CD = Vector2.Lerp(BC, CD, t);

                Vector2 pos = Vector2.Lerp(AB_BC, BC_CD, t);
                mover.position = new Vector3(pos.x, pos.y, 0);
                t = Mathf.Clamp(t + Time.deltaTime * MoveSpeed * 0.1f, 0, 1f);
                yield return null;
            }
            count = (loop == true) ? (count + 1) % path.NumSegment : count + 1;
             t = 0;
            NumSeg = path.NumSegment; //재생중간에, IsClosed 결과값 바꿀경우 대비해서 새로 초기화
        }

    }
}
