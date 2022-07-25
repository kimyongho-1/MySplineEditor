using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class Point //�� ��
{
    [SerializeField] Vector2 Position; //���� ��ġ
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
public class Paths // ������(��) ����, �� ���� ��ġ��, ���� ���õ� �Լ��� ����
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

    public Vector2 this[int i]  // �ε���, �������� ��ġ�� ����������Ƽ
    { get { return Points[i].position; } }

    public int NumPoints { get { return points.Count; } }

    public int NumSegment
    {
        get
        {
            return points.Count / 3;
        }
    }

    public void AddSegment(Vector2 anchorPos) // ���� �ϳ� ��� �� ���� ��Ŀ������ 2���� �߰� �ڵ������� (�Ź� 3���� : ��Ŀ1��+�ڵ�2��)
    {
        points.Add(new Point(points[points.Count - 1].position * 2f - points[points.Count - 2].position));
        points.Add(new Point((points[points.Count - 1].position + anchorPos) * 0.5f)); 
        points.Add(new Point(anchorPos)); 
    }

    public void AddSegmentBetween(Vector2 anchorPos, int segmentIdx) // ������ �߰��� ���� �߰��ϱ�
    {
        // �ΰ��� ���� ������ ������ �ε����� ���۵ȴ�
        // ��0 ��1 ����0 ��2 ��3 ��4 ��5 ����1..
        points.InsertRange(segmentIdx * 3 + 2, new Point[]
        {
            new Point(anchorPos + ((Vector2.down+Vector2.left) * 15f)) ,
            new Point(anchorPos),
            new Point(anchorPos + ((Vector2.up+Vector2.right) * 15f))
        });
        
    }
    public Vector2[] GetPosInSegment(int idx) // ���ڰ��� �ε��� ��ġ���鸸 �̱�
    {
        // �� ������ 4���� ������ �̷�����ִ�
        return new Vector2[] { points[idx * 3].position, points[idx * 3 + 1].position, points[idx * 3 + 2].position, points[LoopIdx(idx * 3 + 3)].position };
    }
    public Point[] GetPointsInSegment(int idx) // �����ε����� �����ϴ� Point.cs�� ��������
    {
        // �� ������ 4���� ������ �̷�����ִ�
        return new Point[] { points[idx * 3], points[idx * 3 + 1], points[idx * 3 + 2], points[LoopIdx(idx * 3 + 3)] };
    }

    public void MovePoint(int idx, Vector2 pos) // ���信�� ���콺�巡�׵����� �� �̵��� ��ġ����
    {
        Vector2 ToMove = pos - points[idx].position; // ����+�Ÿ� ĳ��
        points[idx].position = pos; // �ش� �ε������� �Է�Pos����ŭ �̵���ġ��Ű��

        // ���� 2���� Ÿ���ε�,  1) �������� ����� �ٲ������ "�ڵ���"
        //                     2) �ε�����ġ�� 3�� ����� �����Ǵ� "�߽���"

        if (idx % 3 == 0) // 3�� ���, �߽����� �����ϋ� (���Ʒ� ����� �ڵ���2���� ���� �����̰�����)
        {
            idx = isClosed == true ? LoopIdx(idx) : idx;
            if (isClosed == false)
            {
                if (idx == 0) // idx�� �������� ������, idx�� 0�̴ϱ�  idx-1�� ���ٴ� �Ҹ�
                {
                    points[idx + 1].position += ToMove; return;
                }

                if (idx == points.Count - 1) // idx�� �������� �����ϋ�, idx+1�� ����, ������ �ʰ��ϱ⿡ ����
                {
                    points[idx - 1].position += ToMove; return;
                }

                // �߽����� �����ϋ� ���Ʒ� ����� �ڵ���2���� ���� ��ġ�� �����̰� ���� ������
                points[idx - 1].position += ToMove;
                points[idx + 1].position += ToMove;
                return;
            }

            // ������, ������ �ƴ϶��
            points[LoopIdx(idx - 1)].position += ToMove;
            points[LoopIdx(idx + 1)].position += ToMove;

            return;
        }


        else // �߽����� �ƴ� �ڵ��� �����ϋ�
        {
            // ���� : ���� �ε����� �߽����̶��,
            // �� �߽����� ����� �ڵ����� �ݴ� ȸ���� �־� �����ó�� ȸ��+�̵��� ��Ű�� ����
            
            if (isClosed == false && idx + 1 == points.Count - 1 ||
                isClosed == false && idx - 1 == 0)
            { Debug.Log($"�ε���({idx}) : �ݴ��� �ڵ� ����"); return; }

            // �������� �߽�������? , 3�ǹ���� �߽��� �ε�����ȣ�⿡  1�����ϰ� 3�������� ����� ����
            bool NextIsAnchor = ((idx + 1) % 3 == 0);

            // ���� ����Index�� �߽����� �ƴ϶�� , �����ε���[idx-1]�� �߽����� �ȴ�
            int OppsiteIdx = (NextIsAnchor == true) ? LoopIdx(idx + 2) : LoopIdx(idx - 2);
            // �ݴ��� �ڵ鵵 ������ �����̱����� �߽��� ã��
            int NearAnchorIdx = (NextIsAnchor == true) ? LoopIdx(idx + 1) : idx - 1;

            // ��ó �߽������κ��� ���� �̵��� �ڵ��� ����� �Ÿ��� ���ϰ�
            float dist = Vector2.Distance(points[NearAnchorIdx].position, points[OppsiteIdx].position);
            Vector2 dir = (points[NearAnchorIdx].position - pos).normalized;

            // ������ ������ �Ÿ��� ���� ����Ͽ� �ݴ��� �ڵ��� �ݴ����� ȸ���ϰ� ����
            points[OppsiteIdx].position = points[NearAnchorIdx].position + dir * dist;
        }
    }

    public void ToggleClosed() // ����,�� ������ �̾���̰ų� ���� ����Լ�
    {
        isClosed = !isClosed; // ������Ű��

        if (isClosed == true) // ����,������ �̾����Եȴٸ�
        {
            // ������ ���� ���ο��� ����
            points.Add(new Point( points[points.Count - 1].position * 2f - points[points.Count - 2].position));
            // �������� ������ ���ο��� ����
            points.Add(new Point(points[0].position * 2f - points[1].position));
        }
        else // �ٽ� �������, �߰��ߴ� ���� ���� �����ϸ��
        {
            points.RemoveRange(points.Count - 2, 2);
        }
    }

    int LoopIdx(int idx) // Closed�� ���� 2�� �þ�� ��ü������ �������� ��ȯ�ǵ���
    {
        return (idx + points.Count) % points.Count;
    }

  
    public void ConvertToStraightLine(int Segidx) // ������ ��������, �ΰ��� �ڵ���ġ�� ������ �߽������� �̵����� �������·� �ٲ�
    {
        //points[Segidx*3+1] = points[Segidx*3+2] = Vector2.zero;
        points[Segidx * 3 + 1].position = points[Segidx * 3].position;  // 1���ε����� 0���ε��� ��ġ�� �̵�
        points[Segidx * 3 + 2].position = points[(Segidx + 1) * 3].position;// 2���ε����� 3���ε��� ��ġ�� �̵�

    }

    public void Delete(int idx) // ������ �����
    {
        // IsClosed == false�� ���
        // idx : 0  =>  idx[0,1,2]
        // IsClosed == True , �̾��� Ŀ���� ���
        // idx : 0 || count-1�� ���� ���� ������  =>  idx[ idx,idx+1 ,idx+2] , idx[count-1] = idx[idx+2]

        // idx : count-1  =>  idx[count-1,count-2,count-3,]
        // idx : any  =>  idx[idx-1, idx,idx+1]

        if (NumSegment > 2 || isClosed == false && NumSegment > 1)
        {
            if (idx == 0)  // ���� �ε����� ����ٸ�
            {
                if (isClosed == true)
                { points[points.Count - 1] = points[idx + 2]; }
                points.RemoveRange(0, 3);
            }
            else if (idx == points.Count - 1) // ������ ����ٸ� (������ �߽���)
            {
                points.RemoveRange(idx - 2, 3);
            }

            else // ���۰� �� ���� �ƴ϶�� Closed���� ������� ����
            {
                points.RemoveRange(idx - 1, 3);
            }
        }

    }

    public Point isAnchorPointArea(Vector2 mousePos) // ���� Path�� �ƹ� �߽����� ���콺��ġ���� ���̰� �ټ��ϴٸ� �����߽������� �ִٰ� ����
    {
        for (int i = 0; i < points.Count; i+=3)
        {
            if (Mathf.Abs((points[i].position - mousePos).magnitude) < 10f) { return points[i]; }
        }
        return null;
    }
}
