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
    GenericMenu menuMR ;
    Vector2 mousePos;
    bool showIdx;
    const float NearSeg=5f;
    

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
        menuMR.AddItem(new GUIContent("Alt+Z : UnDo"), false, Empty);
    }

    private void OnSceneGUI()
    {
        Event gui = Event.current; // GUI������ �νĵ�, NULL�����ϱ����� ������ ���콺��ġ���ϴ°� ����
        mousePos = HandleUtility.GUIPointToWorldRay(gui.mousePosition).origin;
        Input();
        Draw();
    }

    public override void OnInspectorGUI()
    {
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
          float dist = (mousePos - _path.points[i]).magnitude;
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
            Vector2[] eachPoint = _path.GetPointsInSegment(i);
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
        _path.AddSegmentBetween(mousePos,selectedIdx);

    }

    void Draw() // PathCreator������Ʈ�� ���� ��ü�� ���� Paths�� ������, ������ ��� �׸���
    {
        for (int i = 0; i < _path.NumSegment; i++)
        {
            Vector2[] relatedPoints = _path.GetPointsInSegment(i);
            Handles.color = _creator.handle; // �ڵ鼱 ����������
            Handles.DrawLine(relatedPoints[0], relatedPoints[1]);
            Handles.DrawLine(relatedPoints[2], relatedPoints[3]);
            // �������� �ʷϻ�����
            Handles.DrawBezier(relatedPoints[0], relatedPoints[3], relatedPoints[1], relatedPoints[2]
                ,_creator.bezier, null, _creator.thickness);
        }

        GUIStyle g = new GUIStyle() { };
        Handles.color = _creator.startAnchor;  // �������� ��������� ǥ�� (���� �������ؼ�)
        for (int i = 0; i < _path.NumPoints; i++)
        {
            if (i == _path.points.Count - 1) { Handles.color = _creator.endAnchor; } //����

            Vector2 newPos = Handles.FreeMoveHandle(
            _path[i] , Quaternion.identity, (i == 0 || i == _path.points.Count-1) ?
            4f * _creator.pointthickness : 2f * _creator.pointthickness,
             Vector2.one , Handles.CylinderHandleCap);

            Handles.color = Color.black;

            if (showIdx == true)  // ���� �ε��� ���信 ǥ�� �������ؼ�
            {
                if (i % 3 == 0) { g.normal.textColor = Color.black; }
                else { g.normal.textColor = Color.white; }
                g.CalcScreenSize(_path[i]); Handles.Label(_path[i], $"{i}", g); 
            }
            
            if (_path[i] != newPos) // ��â���� ���� �����̸� �ǽð����� ��ġ���� �ٸ��ԵǴ�
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
   

