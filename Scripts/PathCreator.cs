using Assets.Script.TweenLibrary;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
[ExecuteAlways]
public class PathCreator : MonoBehaviour  // ������Ʈ ����, ��(Paths.cs)�� ������ ���� ��
{
    [HideInInspector] [SerializeField] Paths Path;
    public Paths path { get { return this.Path;} set { this.Path = value; } }
   [HideInInspector][SerializeField] List<Paths> pathsList = new List<Paths>();
    public Paths allPaths { get { return pathsList[pathsList.Count]; } set { pathsList.Add(value); } }
    public List<Paths> GetList { get { return this.pathsList; } }
    public int Count { get { return pathsList.Count; } }
    
    EffectBuilder effect;
    public Transform mover;
    [Header("�̵��ӵ�")] public float MoveSpeed;
    [Header("���� �ΰ���")] public float _deleteSensibilty;
    [Header("������")] public Color startAnchor;
    [Header("����")] public Color endAnchor;
    [Header("�߽���")] public Color anchor;
    [Header("�ڵ�")] public Color handle;
    [Header("�� ����"), Range(1, 16)] public int pointthickness = 6;
    [Header("������ ��")] public Color bezier;
    [Header("����� ����"), Range(1, 10)] public float thickness;
    bool loop;
    int count, NumSeg = 0;
    int currIdx;
    
    public int CurrPathIdx { get { return currIdx; } set { currIdx = value; } }
  

    private void Start()
    {
        effect = new EffectBuilder(this);
        CurrPathIdx = 0;
        startAnchor.a = endAnchor.a = anchor.a = handle.a = bezier.a = 1; // ����1�� ����
        
        loop = true;
    }

    public void CreatePath() // �н��� Ŀ�긦 ����� ������Ʈ�� Init�Լ����� �� �Լ��� �����Ű��� (Utill.GetOrAddComponent())
    {
        pathsList.Clear();
        path = new Paths(transform.position, 10f);
        pathsList.Add(path);
    }

    public void AddNewPath(Vector2 mousePos) // �������� �н��� ����Ҽ��ֵ���
    {
        pathsList.Add(new Paths(mousePos));
    }
    public void ChangeControlPathIDX(int idx) // ��ųʸ� Ű���� ���� ���ε� ���� �н����� ���� Ű��ȣ���� ����
    {
        path = pathsList[idx]; // ���� ���߰� �� ������� ��Ʈ���� idx��ȣ �н���ü���� �ϱ����
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

    int LoopIdx(int idx) // IsClosed == true, �� ���۰� ���� ����� ���¸� ù�ε����� ���ε����� ��ġ�� �������⿡ ��ȯ�ϰ���
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
            // ����� �̺�Ʈ�Լ��� �ִٸ� ����!
            if (p[3].Effect != null) 
            {
                for (int i = 0; i < p[3].Effect.Count; i++)
                {
                    p[3].Effect[i].Execute();
                }
            }

            // ���� �̵��� ���ѹݺ��������� üũ
            count = (loop == true) ? (count + 1) % path.NumSegment : count + 1;
            // �ð� �ʱ�ȭ 
            t = 0;
            NumSeg = path.NumSegment; //����߰���, IsClosed ����� �ٲܰ�� ����ؼ� ���� �ʱ�ȭ
        }

    }
}
