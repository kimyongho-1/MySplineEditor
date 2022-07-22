
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
        // Mono�� ���� ũ�������� ��ü ����
        pathCreator = new SerializedObject(GameObject.FindObjectOfType<PathCreator>());
        if (pathCreator == null) { Debug.Log("PathCreator.cs����"); }
        // ��밡���� �����͸� ã��(�� Point.cs)
        paths = pathCreator.FindProperty("selectedPath");
    }

    [MenuItem("Window/PathEventShow")] // ������ �гο� PathEventShow�� �޴��߰�
    public static void OpenWindow()
    {
        PathEventWindow window = (PathEventWindow)GetWindow(typeof(PathEventWindow));
        window.minSize = new Vector2(100, 100f); //�ּһ�����
        window.Show(); //������ ���
    }
   
  
    private void OnInspectorUpdate() // 10�����Ӹ��� �ѹ��� ȣ��
    {
        pathCreator.ApplyModifiedProperties();
        pathCreator.Update(); // serializeObject ������Ʈ���־�� ���ŵ�
        
        
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

    void Draw(SerializedProperty property, bool drawChildren) // �׸� ������Ƽ�� �ڽĿ�ұ���?
    {
        string lastPropPath = string.Empty;
        foreach (SerializedProperty p in property)
        {
            if (p.isArray && p.propertyType == SerializedPropertyType.Generic) // �迭+���ʸ�Ÿ��
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

