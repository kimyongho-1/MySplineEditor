//using System.Collections;
//using System.Collections.Generic;
//using UnityEngine;
//using UnityEditor;

//[CustomPropertyDrawer(typeof(Paths))]
//public class PathsDraw : PropertyDrawer // �������̹��� �ƴ� �͵��� �ν����ͷ� �׸������ؼ� ������Ƽ��ο췯���
//{ // �������̹��� ��� �ȹ޴°͵� ��ư�� GUILayOut.Button�� �ƴ϶� GUI.Button
//    SerializedProperty p;
//    SerializedProperty e;
//    SerializedObject pathCreator;
//    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
//    {
//        EditorGUI.BeginProperty(position, label, property);
//        p = property.FindPropertyRelative("Points");

//        Rect foldOutBox = new Rect(position.min.x, position.min.y,
//            position.size.x, EditorGUIUtility.singleLineHeight);

//        property.isExpanded = EditorGUI.Foldout(foldOutBox, property.isExpanded, "Points Elements");

//        if (property.isExpanded)
//        {
//            DrawPosProperty(position);
//            DrawEventProperty(position);
//        }

//        EditorGUI.EndProperty();
//    }
//    public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
//    {

//        int totalLines = property.FindPropertyRelative("Points").arraySize * 3;

//        float lineHeight = EditorGUIUtility.singleLineHeight + 5f;

//        //return base.GetPropertyHeight(property, label);
//        return property.isExpanded ? lineHeight * totalLines : lineHeight;
//    }


//    void DrawPosProperty(Rect rect)
//    {
//        float xPos = rect.min.x;
//        float yPos = rect.min.y + EditorGUIUtility.singleLineHeight; // 2��° ��
//        float width = rect.size.x;
//        float height = EditorGUIUtility.singleLineHeight;

//        Rect DrawArea = new Rect(xPos, yPos, width, height);

//        for (int i = 0; i < p.arraySize; i++)
//        {
//            EditorGUI.PropertyField(DrawArea, p.GetArrayElementAtIndex(i), new GUIContent($"IDX : {i}"), true);
//            DrawArea.y += EditorGUIUtility.singleLineHeight * 3f;
//        }
//    }
//    void DrawEventProperty(Rect rect)
//    {
//        Rect DrawArea = new Rect(rect.min.x + (rect.width * 0.5f),
//            rect.min.y + EditorGUIUtility.singleLineHeight,
//            rect.size.x * 0.5f, EditorGUIUtility.singleLineHeight);
//        if (e == null) { return; }

//        EditorGUI.PropertyField(DrawArea, e, new GUIContent("Event �׸�"));
//    }

//}