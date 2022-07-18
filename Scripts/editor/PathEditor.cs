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

        _creator = (PathCreator)target; // 대상 오브젝트
        if (_creator.path == null)
        { _creator.CreatePath(); } // PathCreator.cs에서 생성함수 실행, 싱글톤처럼 없으면 만들어버리게함

        _path = _creator.path; // PathCreator.cs 내부에 path멤버변수를 에디터cs에도 연결

         menuMR = new GenericMenu();
        menuMR.AddItem(new GUIContent("취소"), false, Empty);
        menuMR.AddItem(new GUIContent("점 추가"), false, AddPoint);
        menuMR.AddItem(new GUIContent("점 삭제"), false, DeletePoint);
        menuMR.AddItem(new GUIContent("선에 점 추가"), false, AddPointAlongSegment);
        menuMR.AddItem(new GUIContent("선을 직선형태로 조정"), false, NoBezier);
        menuMR.AddItem(new GUIContent("독립 선분 생성 (New Paths)"), false, AddNewPath);
        menuMR.AddItem(new GUIContent("Alt+Z : UnDo"), false, Empty);
        eventMR = new GenericMenu();
        eventMR.AddItem(new GUIContent("취소"), false, Empty);
        //eventMR.AddItem(new GUIContent("해당 중심점에 이벤트생성"), false, ); 팝업창으로 클릭한 중심점 정보볼수있게?
        // 후에 AddEffect함수실행
    }

    private void OnSceneGUI()
    {
        Event gui = Event.current; // Event클래스는 GUI에서만 인식됨
        mousePos = HandleUtility.GUIPointToWorldRay(gui.mousePosition).origin; // NULL방지하기위해 언제나 마우스위치값 갱신
        Input();
        Draw();
        _path = _creator.path; // 갈아끼우기
        SceneView.RepaintAll(); // 씬에디터 화면 다시 그리기
    }

    public override void OnInspectorGUI()
    {
        _creator.CurrPathIdx = EditorGUILayout.IntSlider("Segment 선분 변경",_creator.CurrPathIdx, 0, _creator.allPaths.Count);
        base.OnInspectorGUI();
        
        if (GUILayout.Button("Show IndexNumber (인덱스표시 켜기/끄기)")) //씬뷰에 인덱스표시할건지
        {
            showIdx = !showIdx;
        }
        if (GUILayout.Button("Create New")) // 재생성 토글버튼
        {
            _creator.CreatePath(); // 다시 생성
            _path = _creator.path; // 갈아끼우기
            SceneView.RepaintAll(); // 씬에디터 화면 다시 그리기
        }

        if (GUILayout.Button("Toggle Close")) // 인스펙터창에 토글버튼 생성
        {
            _path.ToggleClosed(); // 토글버튼 눌리면 함수실행
            SceneView.RepaintAll(); // 씬에디터 화면 다시 그리기 ( 베지어곡선의 시작과 끝을 잇거나 뗏을때 대비)
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

    void Input() // Repaints는 키나 마우스 입력이 될떄 자동으로 된다고함
    {
        Event gui = Event.current;
      
        // 중심점위에서 우클릭시 이벤트 추가시키기
        if (gui.type == EventType.MouseDown && gui.button == 1 && _path.isAnchorPointArea(mousePos) != null)
        {
            eventMR.ShowAsContext();
            gui.Use(); return;
        }

        // 그저 우클릭시
        if (gui.type == EventType.MouseDown && gui.button == 1)
        {
            menuMR.ShowAsContext();
            gui.Use();
        }

    }

    void AddPoint()
    {
        Undo.RecordObject(_creator, "Add Segment");
        // 마우스지점을 앵커축으로 선분생성(점3개 추가됨)
        _path.AddSegment(mousePos);
    }  // 우클릭 컨텍스트위치에 점 생성
    void DeletePoint() // 우클릭 컨텍스트위치기준 가까운 점 삭제
    {
        float near = 8f; // 가장 가까운 거리값
        int closetIdx = -1; // 혹시 가까운 인덱스를 못찾는다면 에러 뜨도록 -1 값을 넣어버림
        for (int i = 0; i < _path.NumPoints; i += 3)
        {
            float dist = (mousePos - _path.points[i].position).magnitude;
            if (dist < near) { near = dist; closetIdx = i; }
        }

        if (closetIdx == -1) { return; } // 예외처리
        Undo.RecordObject(_creator, "Delete AnchorPoint");
        _path.Delete(closetIdx);
    }
    void AddPointAlongSegment()  // 우클릭 컨텍스트위치기준 가장 가까운 선분에 점 추가
    {
        int selectedIdx = -1;
        float nearDist = NearSeg;
        for (int i = 0; i < _path.NumSegment; i++)
        {
            Vector2[] eachPoint = _path.GetPosInSegment(i);
            // mousePos의 위치와 각선분의 길이 차를 구하기로함
            float dist = HandleUtility.DistancePointBezier(mousePos,
                eachPoint[0], eachPoint[3], eachPoint[1], eachPoint[2]);
            if (dist < nearDist)
            {
                nearDist = dist;
                selectedIdx = i;
            }
        }

        if (selectedIdx == -1) { return; } // 예외처리

        Undo.RecordObject(_creator, "AddPointAlongSegment");
        _path.AddSegmentBetween(mousePos, selectedIdx);

    }
    void AddEvent()
    { }
    void NoBezier() // 우클릭위치에서 가장 가까운 선분을 직선으로(두개의 핸들 정지) 
    {
        int selectedIdx = -1;
        float nearDist = NearSeg;
        for (int i = 0; i < _path.NumSegment; i++)
        {
            Vector2[] eachPoint = _path.GetPosInSegment(i);
            // mousePos의 위치와 각선분의 길이 차를 구하기로함
            float dist = HandleUtility.DistancePointBezier(mousePos,
                eachPoint[0], eachPoint[3], eachPoint[1], eachPoint[2]);
            if (dist < nearDist)
            {
                nearDist = dist;
                selectedIdx = i;
            }
        }

        if (selectedIdx == -1) { return; } // 예외처리

        Undo.RecordObject(_creator, "Turn Off BezierCurve");
        _path.ConvertToStraightLine(selectedIdx);
    }
    void AddNewPath()
    { Debug.Log(mousePos); _creator.AddNewPath(mousePos); }
    void AddEffect()
    {
       // _path.isAnchorPointArea(mousePos).effect = 
    }
    void Draw() // PathCreator컴포넌트를 지닌 객체의 내부 Paths에 접근해, 각점을 모두 그리기
    {
        for (int i = 0; i < _path.NumSegment; i++)
        {
            Point[] relatedPoints = _path.GetPointsInSegment(i);
            Handles.color = _creator.handle; // 핸들점 색지정

            Handles.DrawLine(relatedPoints[0].position, relatedPoints[1].position);
            Handles.DrawLine(relatedPoints[2].position, relatedPoints[3].position);
            // 베지어곡선은 초록색으로
            Handles.DrawBezier(relatedPoints[0].position, relatedPoints[3].position, relatedPoints[1].position, relatedPoints[2].position
                , _creator.bezier, null, _creator.thickness);

        }

        GUIStyle g = new GUIStyle() { };
        Handles.color = _creator.startAnchor;  // 시작점만 노란색으로 표시 (구분 편의위해서)
        for (int i = 0; i < _path.NumPoints; i++)
        {
            if (i == _path.points.Count - 1) { Handles.color = _creator.endAnchor; } //끝점
            Vector2 newPos;

            if (i == 0 || i == _path.points.Count - 1) // 시작점 혹은 끝점이라면
            {
                newPos = Handles.FreeMoveHandle  // 점그리기
                (_path.points[i].position, Quaternion.identity, 4f * _creator.pointthickness, Vector2.one, Handles.CylinderHandleCap);
            }

            else // 그외의 점 그리기
            {
                newPos = Handles.FreeMoveHandle  // 각 중심점 그리기
                (_path.points[i].position, Quaternion.identity,
                // 각 중심점의 크기 : 각 핸들점의 크기
                (i % 3 == 0) ? 3.5f * _creator.pointthickness : 2f * _creator.pointthickness,
                Vector2.one, Handles.CylinderHandleCap);
            }


            Handles.color = Color.black;

            if (showIdx == true)  // 각점 인덱스 씬뷰에 표시 편의위해서
            {
                // if (i % 3 == 0) { g.normal.textColor = Color.black; }
                // else { g.normal.textColor = Color.white; }
                g.normal.textColor = Color.black;
                g.CalcScreenSize(_path.points[i].position); Handles.Label(_path.points[i].position, $"AllPaths[{_creator.CurrPathIdx.ToString()}] , IDX : [{i}]," +
                    $"\n Pos : {_path.points[i].position}", g);
            }

            if (_path.points[i].position != newPos) // 씬창에서 점을 움직이면 실시간으로 위치값이 다르게되니
            {
                Undo.RecordObject(_creator, "Move point");
                _path.MovePoint(i, newPos); // 곧바로 점 위치 수정
            }
            if ((i + 4) % 3 == 0) { Handles.color = _creator.anchor; } // 각 중심점
            else { Handles.color = _creator.handle; } // 각 핸들점
        }
    }
    void Empty() { }
}


