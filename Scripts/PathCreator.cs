using System.Collections;
using System.Collections.Generic;
using UnityEngine;
[ExecuteAlways]
public class PathCreator : MonoBehaviour  // ������Ʈ ����, ��(Paths.cs)�� ������ ���� ��
{
    [HideInInspector]public Paths path;
    public Transform mover;
    [Header("�̵��ӵ�")] public float MoveSpeed;
    [Header("���� �ΰ���")]public float _deleteSensibilty;
    [Header("������")] public Color startAnchor;
    [Header("����")] public Color endAnchor;
    [Header("�߽���")] public Color anchor;
    [Header("�ڵ�")] public Color handle;
    [Header("�� ����"), Range(1, 6)] public int pointthickness;
    [Header("������ ��")] public Color bezier;
    [Header("����� ����"),Range(1,10)] public float thickness;
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

    int LoopIdx(int idx) // IsClosed == true, �� ���۰� ���� ����� ���¸� ù�ε����� ���ε����� ��ġ�� �������⿡ ��ȯ�ϰ���
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
            NumSeg = path.NumSegment; //����߰���, IsClosed ����� �ٲܰ�� ����ؼ� ���� �ʱ�ȭ
        }

    }
}
