using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class Point //각 점
{
    [SerializeField] Vector2 Position; //점의 위치
    public Vector2 position { get { return this.Position; } set { Position = value; } }
    [SerializeField] List<int> ini = new List<int>();
    [SerializeField] List<Effect> effect2 = new List<Effect>(10);
    [SerializeField] List<ScriptableObject> effect3 = new List<ScriptableObject>(10);
    [SerializeField] List<MUI_Effect> effect = new List<MUI_Effect>(1);
    public MUI_Effect AddEffect { set { effect.Add(value); } }
    public List<MUI_Effect> Effect { get { return this.effect; } }
    public Point(Vector2 pos) { position = pos; effect2.Add(new Effect(1)); effect2.Add(new Effect(2)); }
}


[System.Serializable]
public class Paths // 데이터(점) 역할, 각 점의 위치와, 점에 관련된 함수가 있음
{
    [SerializeField] List<Point> Points;
    public List<Point> points { get { return  Points; } }
    [HideInInspector] public bool isClosed;
    public Paths(Vector2 center, float interval = 3f)
    {
        Points = new List<Point>()
        {
           new Point( (center+Vector2.left * interval)),
           new Point( (center+(Vector2.left+Vector2.up)*0.5f )* interval),
           new Point( (center+Vector2.right* interval)),
           new Point( (center+(Vector2.right+Vector2.down)*0.5f)* interval)
        };
    }
    public Paths(Vector2 pos)
    {
        Points = new List<Point>()
        {
          new Point(pos+Vector2.left*5f),
            new Point(pos+Vector2.right*5f),
            new Point(pos+Vector2.left*5f+Vector2.up*8f),
            new Point(pos+Vector2.right*5f+Vector2.up*8f)
        };
     
    }

    public Vector2 this[int i]  // 인덱스, 개별점의 위치값 접근프로퍼티
    { get { return Points[i].position; } }

    public int NumPoints { get { return points.Count; } }

    public int NumSegment
    {
        get
        {
            return points.Count / 3;
        }
    }

    public void AddSegment(Vector2 anchorPos) // 점을 하나 찍고 그 점을 앵커점으로 2개의 추가 핸들점생성 (매번 3개씩 : 앵커1개+핸들2개)
    {
        points.Add(new Point(points[points.Count - 1].position * 2f - points[points.Count - 2].position));
        points.Add(new Point((points[points.Count - 1].position + anchorPos) * 0.5f)); 
        points.Add(new Point(anchorPos)); 
    }

    public void AddSegmentBetween(Vector2 anchorPos, int segmentIdx) // 선분의 중간에 점을 추가하기
    {
        // 두개의 점이 지난후 선분의 인덱스가 시작된다
        // 점0 점1 선분0 점2 점3 점4 점5 선분1..
        points.InsertRange(segmentIdx * 3 + 2, new Point[]
        {
            new Point(anchorPos + ((Vector2.down+Vector2.left) * 15f)) ,
            new Point(anchorPos),
            new Point(anchorPos + ((Vector2.up+Vector2.right) * 15f))
        });
        
    }
    public Vector2[] GetPosInSegment(int idx) // 인자값의 인덱스 위치값들만 뽑기
    {
        // 각 선분은 4개의 점으로 이루어져있다
        return new Vector2[] { points[idx * 3].position, points[idx * 3 + 1].position, points[idx * 3 + 2].position, points[LoopIdx(idx * 3 + 3)].position };
    }
    public Point[] GetPointsInSegment(int idx) // 선분인덱스를 구성하는 Point.cs다 꺼내오기
    {
        // 각 선분은 4개의 점으로 이루어져있다
        return new Point[] { points[idx * 3], points[idx * 3 + 1], points[idx * 3 + 2], points[LoopIdx(idx * 3 + 3)] };
    }

    public void MovePoint(int idx, Vector2 pos) // 씬뷰에서 마우스드래그등으로 점 이동후 위치갱신
    {
        Vector2 ToMove = pos - points[idx].position; // 방향+거리 캐싱
        points[idx].position = pos; // 해당 인덱스점을 입력Pos값만큼 이동위치시키기

        // 점은 2가지 타입인데,  1) 베지어곡선의 모양을 바꿔버리는 "핸들점"
        //                     2) 인덱스수치가 3의 배수로 측정되는 "중심점"

        if (idx % 3 == 0) // 3의 배수, 중심점을 움직일떄 (위아래 연결된 핸들점2개도 같이 움직이게유도)
        {
            idx = isClosed == true ? LoopIdx(idx) : idx;
            if (isClosed == false)
            {
                if (idx == 0) // idx가 베지어의 시작점, idx가 0이니까  idx-1이 없다는 소리
                {
                    points[idx + 1].position += ToMove; return;
                }

                if (idx == points.Count - 1) // idx가 베지어의 끝점일떄, idx+1이 없음, 범위를 초과하기에 주의
                {
                    points[idx - 1].position += ToMove; return;
                }

                // 중심점이 움직일떄 위아래 연결된 핸들점2개도 같은 위치로 움직이게 현재 유도함
                points[idx - 1].position += ToMove;
                points[idx + 1].position += ToMove;
                return;
            }

            // 시작점, 끝점이 아니라면
            points[LoopIdx(idx - 1)].position += ToMove;
            points[LoopIdx(idx + 1)].position += ToMove;

            return;
        }


        else // 중심점이 아닌 핸들을 움직일떄
        {
            // 목적 : 다음 인덱스가 중심점이라면,
            // 그 중심점에 연결된 핸들점에 반대 회전을 주어 막대기처럼 회전+이동을 시키게 유도
            
            if (isClosed == false && idx + 1 == points.Count - 1 ||
                isClosed == false && idx - 1 == 0)
            { Debug.Log($"인덱스({idx}) : 반대편 핸들 없음"); return; }

            // 다음점이 중심점인지? , 3의배수가 중심점 인덱스번호기에  1을더하고 3나머지한 결과로 보기
            bool NextIsAnchor = ((idx + 1) % 3 == 0);

            // 만약 다음Index가 중심점이 아니라면 , 이전인덱스[idx-1]가 중심점이 된다
            int OppsiteIdx = (NextIsAnchor == true) ? LoopIdx(idx + 2) : LoopIdx(idx - 2);
            // 반대편 핸들도 역으로 움직이기위해 중심점 찾기
            int NearAnchorIdx = (NextIsAnchor == true) ? LoopIdx(idx + 1) : idx - 1;

            // 근처 중심점으로부터 현재 이동한 핸들의 방향과 거리를 구하고
            float dist = Vector2.Distance(points[NearAnchorIdx].position, points[OppsiteIdx].position);
            Vector2 dir = (points[NearAnchorIdx].position - pos).normalized;

            // 방향을 뒤집어 거리를 곱해 계산하여 반대편 핸들을 반대으로 회전하게 유도
            points[OppsiteIdx].position = points[NearAnchorIdx].position + dir * dist;
        }
    }

    public void ToggleClosed() // 시작,끝 지점을 이어붙이거나 떼는 토글함수
    {
        isClosed = !isClosed; // 반전시키기

        if (isClosed == true) // 시작,끝점이 이어지게된다면
        {
            // 끝점의 뒷편에 새로운점 생성
            points.Add(new Point( points[points.Count - 1].position * 2f - points[points.Count - 2].position));
            // 시작점의 뒷편에도 새로운점 생성
            points.Add(new Point(points[0].position * 2f - points[1].position));
        }
        else // 다시 찢어놓기, 추가했던 점을 그저 삭제하면됨
        {
            points.RemoveRange(points.Count - 2, 2);
        }
    }

    int LoopIdx(int idx) // Closed로 점이 2개 늘어나도 전체갯수의 나머지로 순환되도록
    {
        return (idx + points.Count) % points.Count;
    }

  
    public void ConvertToStraightLine(int Segidx) // 선분을 기준으로, 두개의 핸들위치를 강제로 중심점으로 이동시켜 선분형태로 바꿈
    {
        //points[Segidx*3+1] = points[Segidx*3+2] = Vector2.zero;
        points[Segidx * 3 + 1].position = points[Segidx * 3].position;  // 1번인덱스를 0번인덱스 위치로 이동
        points[Segidx * 3 + 2].position = points[(Segidx + 1) * 3].position;// 2번인덱스를 3번인덱스 위치로 이동

    }

    public void Delete(int idx) // 선택점 지우기
    {
        // IsClosed == false의 경우
        // idx : 0  =>  idx[0,1,2]
        // IsClosed == True , 이어진 커브의 경우
        // idx : 0 || count-1은 서로 같은 포지션  =>  idx[ idx,idx+1 ,idx+2] , idx[count-1] = idx[idx+2]

        // idx : count-1  =>  idx[count-1,count-2,count-3,]
        // idx : any  =>  idx[idx-1, idx,idx+1]

        if (NumSegment > 2 || isClosed == false && NumSegment > 1)
        {
            if (idx == 0)  // 시작 인덱스를 지운다면
            {
                if (isClosed == true)
                { points[points.Count - 1] = points[idx + 2]; }
                points.RemoveRange(0, 3);
            }
            else if (idx == points.Count - 1) // 끝점을 지운다면 (끝점도 중심점)
            {
                points.RemoveRange(idx - 2, 3);
            }

            else // 시작과 끝 점이 아니라면 Closed여부 상관없이 동일
            {
                points.RemoveRange(idx - 1, 3);
            }
        }

    }

    public Point isAnchorPointArea(Vector2 mousePos) // 현재 Path의 아무 중심점과 마우스위치값의 차이가 근소하다면 현재중심점위에 있다고 판정
    {
        for (int i = 0; i < points.Count; i+=3)
        {
            if (Mathf.Abs((points[i].position - mousePos).magnitude) < 10f) { return points[i]; }
        }
        return null;
    }
}
