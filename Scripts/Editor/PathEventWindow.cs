
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

public class PathEventWindow : EditorWindow
{
    SerializedObject pathCreator;
    SerializedProperty paths;
    PathCreator _creator;

    private void OnEnable()
    {
        _creator = GameObject.FindObjectOfType<PathCreator>();
        // Mono를 가진 크리에이터 객체 생성
        pathCreator = new SerializedObject(GameObject.FindObjectOfType<PathCreator>());
        if (pathCreator == null) { Debug.Log("PathCreator.cs없음"); }
        // 모노가없는 데이터를 찾기(점 Point.cs)
        paths = pathCreator.FindProperty("selectedPath");
    }

    [MenuItem("Window/PathEventShow")] // 윈도우 패널에 PathEventShow란 메뉴추가
    public static void OpenWindow()
    {
        PathEventWindow window = (PathEventWindow)GetWindow(typeof(PathEventWindow));
        window.minSize = new Vector2(100, 100f); //최소사이즈
        window.Show(); //윈도우 재생
    }
   
  
    private void OnInspectorUpdate() // 10프레임마다 한번씩 호출
    {
        pathCreator.ApplyModifiedProperties();
        pathCreator.Update(); // serializeObject 업데이트해주어야 갱신됨
        
        
    }

    private void OnGUI()
    {
        Debug.Log("this "+ paths.FindPropertyRelative("Points"));
        string[] n= new string[_creator.PathList.Count];
        int[] j = new int[_creator.PathList.Count];
        for (int i = 0; i < _creator.PathList.Count; i++)
        {
            n[i] = "Path Index : " + i;
            j[i] = i;
        }
        
        EditorGUILayout.Space(10);
        GUILayout.Label("Selection Filter", EditorStyles.boldLabel);
        _creator.CurrPathIdx = EditorGUILayout.IntPopup(_creator.CurrPathIdx, n, j);
        
        if (_creator.PathList.Count > 0) { _creator.ChangeControlPathIDX(_creator.CurrPathIdx); }
     
       EditorGUILayout.BeginHorizontal();
       paths.isExpanded = EditorGUILayout.Foldout(paths.isExpanded, "Points Elements");
       EditorGUILayout.EndHorizontal();
       if (paths.isExpanded)
       {
           for (int i = 0; i < paths.FindPropertyRelative("Points").arraySize; i++)
           {
               EditorGUILayout.PropertyField(paths.FindPropertyRelative("Points").GetArrayElementAtIndex(i)
               , new GUIContent($"IDX : {i}"), true);
           }
       }

    }

    void Draw(SerializedProperty property, bool drawChildren) // 그릴 프로퍼티와 자식요소까지?
    {
        string lastPropPath = string.Empty;
        foreach (SerializedProperty p in property)
        {
            if (p.isArray && p.propertyType == SerializedPropertyType.Generic) // 배열+제너릭타입
            {
                EditorGUILayout.BeginHorizontal();
                p.isExpanded = EditorGUILayout.Foldout(p.isExpanded, p.displayName);
                EditorGUILayout.EndHorizontal();

                if (p.isExpanded)
                {
                    EditorGUI.indentLevel++;
                    Draw(p, true);
                    EditorGUI.indentLevel--;
                }
            }

            else 
            {
                if (!string.IsNullOrEmpty(lastPropPath) && p.propertyPath.Contains(lastPropPath)) { continue; }
                lastPropPath = p.propertyPath;
                EditorGUILayout.PropertyField(p, true);
            }
        }
    }
}

