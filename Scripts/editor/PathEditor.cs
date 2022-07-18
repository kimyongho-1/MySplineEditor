using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(PathCreator))]
public class PathEditor : Editor
{
    PathCreator _creator;
    Paths _path;
    GenericMenu menuMR,eventMR;
    Vector2 mousePos;
    bool showIdx = true;
    const float NearSeg = 5f;

    private void OnEnable()
    {

        _creator = (PathCreator)target; // ��� ������Ʈ
        if (_creator.path == null)
        { _creator.CreatePath(); } // PathCreator.cs���� �����Լ� ����, �̱���ó�� ������ ������������

        _path = _creator.path; // PathCreator.cs ���ο� path��������� ������cs���� ����

         menuMR = new GenericMenu();
        menuMR.AddItem(new GUIContent("���"), false, Empty);
        menuMR.AddItem(new GUIContent("�� �߰�"), false, AddPoint);
        menuMR.AddItem(new GUIContent("�� ����"), false, DeletePoint);
        menuMR.AddItem(new GUIContent("���� �� �߰�"), false, AddPointAlongSegment);
        menuMR.AddItem(new GUIContent("���� �������·� ����"), false, NoBezier);
        menuMR.AddItem(new GUIContent("���� ���� ���� (New Paths)"), false, AddNewPath);
        menuMR.AddItem(new GUIContent("Alt+Z : UnDo"), false, Empty);
        eventMR = new GenericMenu();
        eventMR.AddItem(new GUIContent("���"), false, Empty);
        //eventMR.AddItem(new GUIContent("�ش� �߽����� �̺�Ʈ����"), false, ); �˾�â���� Ŭ���� �߽��� ���������ְ�?
        // �Ŀ� AddEffect�Լ�����
    }

    private void OnSceneGUI()
    {
        Event gui = Event.current; // EventŬ������ GUI������ �νĵ�
        mousePos = HandleUtility.GUIPointToWorldRay(gui.mousePosition).origin; // NULL�����ϱ����� ������ ���콺��ġ�� ����
        Input();
        Draw();
        _path = _creator.path; // ���Ƴ����
        SceneView.RepaintAll(); // �������� ȭ�� �ٽ� �׸���
    }

    public override void OnInspectorGUI()
    {
        _creator.CurrPathIdx = EditorGUILayout.IntSlider("Segment ���� ����",_creator.CurrPathIdx, 0, _creator.allPaths.Count);
        base.OnInspectorGUI();
        
        if (GUILayout.Button("Show IndexNumber (�ε���ǥ�� �ѱ�/����)")) //���信 �ε���ǥ���Ұ���
        {
            showIdx = !showIdx;
        }
        if (GUILayout.Button("Create New")) // ����� ��۹�ư
        {
            _creator.CreatePath(); // �ٽ� ����
            _path = _creator.path; // ���Ƴ����
            SceneView.RepaintAll(); // �������� ȭ�� �ٽ� �׸���
        }

        if (GUILayout.Button("Toggle Close")) // �ν�����â�� ��۹�ư ����
        {
            _path.ToggleClosed(); // ��۹�ư ������ �Լ�����
            SceneView.RepaintAll(); // �������� ȭ�� �ٽ� �׸��� ( �������� ���۰� ���� �հų� ������ ���)
        }
        if (GUILayout.Button("Do Move"))
        {
            _creator.DoMove();
        }
        if (GUILayout.Button("Cancel Move"))
        {
            _creator.CancelMove();
        }

    }

    void Input() // Repaints�� Ű�� ���콺 �Է��� �ɋ� �ڵ����� �ȴٰ���
    {
        Event gui = Event.current;
      
        // �߽��������� ��Ŭ���� �̺�Ʈ �߰���Ű��
        if (gui.type == EventType.MouseDown && gui.button == 1 && _path.isAnchorPointArea(mousePos) != null)
        {
            eventMR.ShowAsContext();
            gui.Use(); return;
        }

        // ���� ��Ŭ����
        if (gui.type == EventType.MouseDown && gui.button == 1)
        {
            menuMR.ShowAsContext();
            gui.Use();
        }

    }

    void AddPoint()
    {
        Undo.RecordObject(_creator, "Add Segment");
        // ���콺������ ��Ŀ������ ���л���(��3�� �߰���)
        _path.AddSegment(mousePos);
    }  // ��Ŭ�� ���ؽ�Ʈ��ġ�� �� ����
    void DeletePoint() // ��Ŭ�� ���ؽ�Ʈ��ġ���� ����� �� ����
    {
        float near = 8f; // ���� ����� �Ÿ���
        int closetIdx = -1; // Ȥ�� ����� �ε����� ��ã�´ٸ� ���� �ߵ��� -1 ���� �־����
        for (int i = 0; i < _path.NumPoints; i += 3)
        {
            float dist = (mousePos - _path.points[i].position).magnitude;
            if (dist < near) { near = dist; closetIdx = i; }
        }

        if (closetIdx == -1) { return; } // ����ó��
        Undo.RecordObject(_creator, "Delete AnchorPoint");
        _path.Delete(closetIdx);
    }
    void AddPointAlongSegment()  // ��Ŭ�� ���ؽ�Ʈ��ġ���� ���� ����� ���п� �� �߰�
    {
        int selectedIdx = -1;
        float nearDist = NearSeg;
        for (int i = 0; i < _path.NumSegment; i++)
        {
            Vector2[] eachPoint = _path.GetPosInSegment(i);
            // mousePos�� ��ġ�� �������� ���� ���� ���ϱ����
            float dist = HandleUtility.DistancePointBezier(mousePos,
                eachPoint[0], eachPoint[3], eachPoint[1], eachPoint[2]);
            if (dist < nearDist)
            {
                nearDist = dist;
                selectedIdx = i;
            }
        }

        if (selectedIdx == -1) { return; } // ����ó��

        Undo.RecordObject(_creator, "AddPointAlongSegment");
        _path.AddSegmentBetween(mousePos, selectedIdx);

    }
    void AddEvent()
    { }
    void NoBezier() // ��Ŭ����ġ���� ���� ����� ������ ��������(�ΰ��� �ڵ� ����) 
    {
        int selectedIdx = -1;
        float nearDist = NearSeg;
        for (int i = 0; i < _path.NumSegment; i++)
        {
            Vector2[] eachPoint = _path.GetPosInSegment(i);
            // mousePos�� ��ġ�� �������� ���� ���� ���ϱ����
            float dist = HandleUtility.DistancePointBezier(mousePos,
                eachPoint[0], eachPoint[3], eachPoint[1], eachPoint[2]);
            if (dist < nearDist)
            {
                nearDist = dist;
                selectedIdx = i;
            }
        }

        if (selectedIdx == -1) { return; } // ����ó��

        Undo.RecordObject(_creator, "Turn Off BezierCurve");
        _path.ConvertToStraightLine(selectedIdx);
    }
    void AddNewPath()
    { Debug.Log(mousePos); _creator.AddNewPath(mousePos); }
    void AddEffect()
    {
       // _path.isAnchorPointArea(mousePos).effect = 
    }
    void Draw() // PathCreator������Ʈ�� ���� ��ü�� ���� Paths�� ������, ������ ��� �׸���
    {
        for (int i = 0; i < _path.NumSegment; i++)
        {
            Point[] relatedPoints = _path.GetPointsInSegment(i);
            Handles.color = _creator.handle; // �ڵ��� ������

            Handles.DrawLine(relatedPoints[0].position, relatedPoints[1].position);
            Handles.DrawLine(relatedPoints[2].position, relatedPoints[3].position);
            // �������� �ʷϻ�����
            Handles.DrawBezier(relatedPoints[0].position, relatedPoints[3].position, relatedPoints[1].position, relatedPoints[2].position
                , _creator.bezier, null, _creator.thickness);

        }

        GUIStyle g = new GUIStyle() { };
        Handles.color = _creator.startAnchor;  // �������� ��������� ǥ�� (���� �������ؼ�)
        for (int i = 0; i < _path.NumPoints; i++)
        {
            if (i == _path.points.Count - 1) { Handles.color = _creator.endAnchor; } //����
            Vector2 newPos;

            if (i == 0 || i == _path.points.Count - 1) // ������ Ȥ�� �����̶��
            {
                newPos = Handles.FreeMoveHandle  // ���׸���
                (_path.points[i].position, Quaternion.identity, 4f * _creator.pointthickness, Vector2.one, Handles.CylinderHandleCap);
            }

            else // �׿��� �� �׸���
            {
                newPos = Handles.FreeMoveHandle  // �� �߽��� �׸���
                (_path.points[i].position, Quaternion.identity,
                // �� �߽����� ũ�� : �� �ڵ����� ũ��
                (i % 3 == 0) ? 3.5f * _creator.pointthickness : 2f * _creator.pointthickness,
                Vector2.one, Handles.CylinderHandleCap);
            }


            Handles.color = Color.black;

            if (showIdx == true)  // ���� �ε��� ���信 ǥ�� �������ؼ�
            {
                // if (i % 3 == 0) { g.normal.textColor = Color.black; }
                // else { g.normal.textColor = Color.white; }
                g.normal.textColor = Color.black;
                g.CalcScreenSize(_path.points[i].position); Handles.Label(_path.points[i].position, $"AllPaths[{_creator.CurrPathIdx.ToString()}] , IDX : [{i}]," +
                    $"\n Pos : {_path.points[i].position}", g);
            }

            if (_path.points[i].position != newPos) // ��â���� ���� �����̸� �ǽð����� ��ġ���� �ٸ��ԵǴ�
            {
                Undo.RecordObject(_creator, "Move point");
                _path.MovePoint(i, newPos); // ��ٷ� �� ��ġ ����
            }
            if ((i + 4) % 3 == 0) { Handles.color = _creator.anchor; } // �� �߽���
            else { Handles.color = _creator.handle; } // �� �ڵ���
        }
    }
    void Empty() { }
}


