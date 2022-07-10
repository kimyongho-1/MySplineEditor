using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class Paths // ������(��) ����, �� ���� ��ġ��, ���� ���õ� �Լ��� ����
{
    [HideInInspector] public List<Vector2> points;
    [HideInInspector] public List<List<Vector2>> segments;
    [HideInInspector] public bool isClosed;
    public Paths(Vector2 center,float interval = 3f)
    {
        points = new List<Vector2>()
        { 
            (center+Vector2.left) * interval,
            (center+(Vector2.left+Vector2.up)*0.5f )* interval,
            (center+Vector2.right)* interval,
            (center+(Vector2.right+Vector2.down)*0.5f)* interval
        };

        segments = new List<List<Vector2>>() { points};
    }

    public Vector2 this[int i]  // �ε���, ������
    { get { return points[i]; }    }

    public int NumPoints { get { return points.Count; } }

    public int NumSegment
    {
        get 
        {
            return  points.Count / 3; 
        } 
    }

    public void AddSegment(Vector2 anchorPos) // ���� �ϳ� ��� �� ���� ��Ŀ������ 2���� �߰� �ڵ������� (�Ź� 3���� : ��Ŀ1��+�ڵ�2��)
    {
        points.Add(points[points.Count-1]*2f - points[points.Count - 2]);
        points.Add((points[points.Count - 1] + anchorPos) * 0.5f);
        points.Add(anchorPos);
      //points.Add(  (anchorPos + points[points.Count - 1]) *0.5f);
      //points.Add( anchorPos - points[points.Count - 3] );
    }

    public void AddSegmentBetween(Vector2 anchorPos, int segmentIdx) // ������ �߰��� ���� �߰��ϱ�
    {
        // �ΰ��� ���� ������ ������ �ε����� ���۵ȴ�
        // ��0 ��1 ����0 ��2 ��3 ��4 ��5 ����1..
        points.InsertRange(segmentIdx*3+2, new Vector2[]
        {
          anchorPos + ((Vector2.down+Vector2.left) * 15f),
          anchorPos ,
          anchorPos + ((Vector2.up+Vector2.right) * 15f)
        });

    }

    public Vector2[] GetPointsInSegment(int idx) // ���ڴ� ������ �߽��� �ε���
    {
        // �� ������ 4���� ������ �̷�����ִ�
        return new Vector2[] { points[idx * 3], points[idx * 3 + 1], points[idx * 3 + 2], points[LoopIdx(idx * 3 + 3)] };
    }

    public void MovePoint(int idx, Vector2 pos) // ���信�� ���콺�巡�׵����� �� �̵��� ��ġ����
    {
        Vector2 ToMove = pos - points[idx]; // ����+�Ÿ� ĳ��
        points[idx] = pos; // �ش� �ε������� �Է�Pos����ŭ �̵���ġ��Ű��

        // ���� 2���� Ÿ���ε�,  1) �������� ����� �ٲ������ "�ڵ���"
        //                     2) �ε�����ġ�� 3�� ����� �����Ǵ� "�߽���"

        if (idx % 3 == 0) // 3�� ���, �߽����� �����ϋ� (���Ʒ� ����� �ڵ���2���� ���� �����̰�����)
        {
            idx = isClosed == true ? LoopIdx(idx) : idx ;
            if (isClosed == false)
            { 
               if ( idx == 0 ) // idx�� �������� ������, idx�� 0�̴ϱ�  idx-1�� ���ٴ� �Ҹ�
               { points[idx + 1] += ToMove; return; }
             
               if ( idx == points.Count - 1 ) // idx�� �������� �����ϋ�, idx+1�� ����, ������ �ʰ��ϱ⿡ ����
               { points[idx - 1] += ToMove; return; }
                // �߽����� �����ϋ� ���Ʒ� ����� �ڵ���2���� ���� ��ġ�� �����̰� ���� ������
                points[idx - 1] += ToMove;
                points[idx + 1] += ToMove;
                return;
            }
            // ������, ������ �ƴ϶��
            points[LoopIdx(idx - 1) ] += ToMove;
            points[LoopIdx(idx + 1) ] += ToMove;

            return;
        }


        else // �߽����� �ƴ� �ڵ��� �����ϋ�
        {
            // ���� : ���� �ε����� �߽����̶��,
            // �� �߽����� ����� �ڵ����� �ݴ� ȸ���� �־� �����ó�� ȸ��+�̵��� ��Ű�� ����
            Debug.Log(isClosed);
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
            float dist = Vector2.Distance(points[NearAnchorIdx], points[OppsiteIdx]);
            Vector2 dir = (points[NearAnchorIdx] - pos).normalized;

            // ������ ������ �Ÿ��� ���� ����Ͽ� �ݴ��� �ڵ��� �ݴ����� ȸ���ϰ� ����
            points[OppsiteIdx] = points[NearAnchorIdx] + dir * dist; 
        }
    }

    public void ToggleClosed() // ����,�� ������ �̾���̰ų� ���� ����Լ�
    {
        isClosed = !isClosed; // ������Ű��

        if (isClosed == true) // ����,������ �̾����Եȴٸ�
        {
            // ������ ���� ���ο��� ����
            points.Add(points[points.Count - 1] * 2f - points[points.Count - 2]);
            // �������� ������ ���ο��� ����
            points.Add(points[0] * 2f - points[1]);
        }
        else // �ٽ� �������, �߰��ߴ� ���� ���� �����ϸ��
        {
            points.RemoveRange(points.Count-2, 2);
        }
    }

    int LoopIdx(int idx) // Closed�� ���� 2�� �þ�� ��ü������ �������� ��ȯ�ǵ���
    {
        return (idx + points.Count) % points.Count;
    }

    void Set()
    {
        float[] array = new float[points.Count];

        for (int i = 0; i < points.Count; i++)
        {
            array.SetValue((points[LoopIdx(i + 1)] - points[LoopIdx(i)]).magnitude, i);
        }

        float sum = 0 ;
        for (int i = 0; i < array.Length; i++)
        {
            sum += array[i];
        }

        float dist = sum / NumSegment;

        for (int i = 0; i < points.Count; i+=3) // �߽�������
        {
          
            Vector2 dir = (points[LoopIdx(i + 1)] - points[LoopIdx(i)]);
            points[LoopIdx(i)] = dir.normalized * dist;
        }

        // �߽��� �� ���ϸ� �ڵ����ϴ�

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
                points.RemoveRange(idx-1,3);
            }
        }

    }

    

    public void Split() // ��ó ��Ŀ���� �������� ���� �ɰ���
    { 
        
    }
}
