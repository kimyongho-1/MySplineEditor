using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

public class TestWindow : EditorWindow
{
    SerializedObject so;
    SerializedProperty sp,sp2;
    [MenuItem("Window/TestShow")] // ������ �гο� PathEventShow�� �޴��߰�
    public static void Open()
    {
        TestWindow window = GetWindow<TestWindow>();
        window.minSize = new Vector2(100, 100f); //�ּһ�����
        window.Show(); //������ ���
    }
    private void OnInspectorUpdate() // 10�����Ӹ��� �ѹ��� ȣ��
    {
        so.Update(); // serializeObject ������Ʈ���־�� ���ŵ�
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
