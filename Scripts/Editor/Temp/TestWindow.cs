using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

public class TestWindow : EditorWindow
{
    SerializedObject so;
    SerializedProperty sp,sp2;
    [MenuItem("Window/TestShow")] // 윈도우 패널에 PathEventShow란 메뉴추가
    public static void Open()
    {
        TestWindow window = GetWindow<TestWindow>();
        window.minSize = new Vector2(100, 100f); //최소사이즈
        window.Show(); //윈도우 재생
    }
    private void OnInspectorUpdate() // 10프레임마다 한번씩 호출
    {
        so.Update(); // serializeObject 업데이트해주어야 갱신됨
        so.ApplyModifiedProperties();
    }
    private void OnEnable()
    {
        so = new SerializedObject(GameObject.FindObjectOfType<Test>());
        if (so == null) { Debug.Log("SO is null!!!"); }
        sp = so.FindProperty("list");//.FindPropertyRelative("ele");
        sp2 = so.FindProperty("dic");
    }

    public void OnGUI()
    {
        GUILayout.Label("Selection Filter", EditorStyles.boldLabel);
        EditorGUILayout.BeginHorizontal();
        
        EditorGUILayout.EndHorizontal();
      
        for (int i = 0; i < sp.arraySize; i++)
        {
            EditorGUILayout.PropertyField(sp.GetArrayElementAtIndex(i)
            , new GUIContent($"IDX : {i}"), true);
        }
       
    }
}
