using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
public class PathWindowEditor : EditorWindow
{
    SerializedObject pathCreator;
    SerializedProperty pathsList;
    SerializedProperty path;
    PathCreator _creator;


    public static void Open()
    {
        PathWindowEditor window = (PathWindowEditor)GetWindow<PathWindowEditor>();
        window.pathCreator = new SerializedObject(FindObjectOfType<PathCreator>());
        if (window.pathCreator == null) { Debug.Log("Creator null"); }

        window.pathsList = window.pathCreator.FindProperty("pathsList");
        if (window.pathsList == null) { Debug.Log("pathsList null"); }

        window.path = window.pathCreator.FindProperty("Path");
        if (window.path == null) { Debug.Log("path null"); }
        window._creator = FindObjectOfType<PathCreator>();
        if (window._creator == null) { Debug.Log("creator null"); }

        window.minSize = new Vector2(100, 100f); //최소사이즈
        window.Show(); //윈도우 재생
    }
    private void OnInspectorUpdate()
    {
        pathCreator.ApplyModifiedProperties();
        pathCreator.Update();
    }
    private void OnGUI()
    {
        Debug.Log(pathsList.arraySize);
        string[] n = new string[pathsList.arraySize];
        int[] j = new int[pathsList.arraySize];
        for (int i = 0; i < pathsList.arraySize; i++)
        {
            n[i] = "Path Index : " + i;
            j[i] = i;
        }

        EditorGUILayout.Space(10);
        GUILayout.Label("Selection Filter", EditorStyles.boldLabel);
       
        if (_creator.Count > 0)
        {
            _creator.CurrPathIdx = EditorGUILayout.IntPopup(_creator.CurrPathIdx, n, j);
            _creator.ChangeControlPathIDX(_creator.CurrPathIdx);
        }

        EditorGUILayout.BeginHorizontal();
        path.isExpanded = EditorGUILayout.Foldout(path.isExpanded, "Points Elements");
        EditorGUILayout.EndHorizontal();
        Debug.Log((pathsList.GetArrayElementAtIndex(_creator.CurrPathIdx)
       .FindPropertyRelative("Points").GetArrayElementAtIndex(0).FindPropertyRelative("effect2").GetArrayElementAtIndex(0).FindPropertyRelative("pos")));
    
        if (path.isExpanded)
        {
            
            for (int i = 0; i < path.FindPropertyRelative("Points").arraySize; i++)
            {
               // EditorGUILayout.PropertyField(path.FindPropertyRelative("Points").GetArrayElementAtIndex(i), new GUIContent($"IDX : {i}"), true);
                SerializedProperty pointVec = pathsList.GetArrayElementAtIndex(_creator.CurrPathIdx)
                    .FindPropertyRelative("Points").GetArrayElementAtIndex(i).FindPropertyRelative("Position");
                Vector2 pos = pointVec.vector2Value;
                pos = EditorGUILayout.Vector2Field($"idx[{i}] Pos", pos);
                pointVec.vector2Value = pos;
                EditorGUILayout.PropertyField(pathsList.GetArrayElementAtIndex(_creator.CurrPathIdx)
                .FindPropertyRelative("Points").GetArrayElementAtIndex(i).FindPropertyRelative("effect2"));
                for (int k = 0; k < pathsList.GetArrayElementAtIndex(_creator.CurrPathIdx)
                    .FindPropertyRelative("Points").GetArrayElementAtIndex(i).FindPropertyRelative("effect2").arraySize; k++)
                {
                    EditorGUILayout.PropertyField(pathsList.GetArrayElementAtIndex(_creator.CurrPathIdx)
                    .FindPropertyRelative("Points").GetArrayElementAtIndex(i).FindPropertyRelative("effect2").GetArrayElementAtIndex(k).FindPropertyRelative("pos"));
                    EditorGUILayout.PropertyField(pathsList.GetArrayElementAtIndex(_creator.CurrPathIdx)
                        .FindPropertyRelative("Points").GetArrayElementAtIndex(i).FindPropertyRelative("effect2").GetArrayElementAtIndex(k).FindPropertyRelative("speed"));
                }
                

            }

        }

    }
}
