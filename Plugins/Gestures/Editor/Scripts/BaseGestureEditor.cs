using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace Leap.Unity.GestureMachine
{
    [CustomEditor(typeof(BaseGesture), editorForChildClasses: true), CanEditMultipleObjects]
    public class BaseGestureEditor : Editor
    {
        GUIStyle _resultTrue, _resultFalse, _wordWrap;

        private void Awake()
        {
            _resultFalse = new GUIStyle(EditorStyles.boldLabel);
            _resultTrue = new GUIStyle(EditorStyles.boldLabel);
            _resultFalse.normal.textColor = Color.red;
            _resultTrue.normal.textColor = Color.green;
            _wordWrap = new GUIStyle(EditorStyles.boldLabel);
            _wordWrap.wordWrap = true;
        }

        public override void OnInspectorGUI()
        {
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            SerializedProperty gestureOrder = serializedObject.FindProperty("_gestureOrder");
            if (gestureOrder.arraySize > 0)
            {
                if (gestureOrder.arraySize == 1 && gestureOrder.GetArrayElementAtIndex(0).intValue == -2)
                {
                    EditorGUILayout.LabelField("Gesture Starting Point", EditorStyles.boldLabel);
                }
                else
                {
                    string indexString = "Gesture Index: ";

                    for (int i = 0; i < gestureOrder.arraySize; i++)
                    {
                        indexString += gestureOrder.GetArrayElementAtIndex(i).intValue + 1;
                        if (i < gestureOrder.arraySize - 1)
                        {
                            indexString += ", ";
                        }
                    }
                    EditorGUILayout.LabelField(indexString, EditorStyles.boldLabel);
                }
            }
            if (Application.isPlaying)
            {
                SerializedProperty result = serializedObject.FindProperty("result");
                SerializedProperty value = serializedObject.FindProperty("value");

                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.LabelField($"Result: {(result.boolValue ? "✓" : "✕")}", result.boolValue ? _resultTrue : _resultFalse);
                EditorGUILayout.LabelField($"Value: {value.floatValue.ToString("0.###")}", result.boolValue ? _resultTrue : _resultFalse);
                EditorGUILayout.EndHorizontal();
            }
            EditorGUILayout.EndVertical();
            SerializedProperty childGestures = serializedObject.FindProperty("childGestures");
            if (childGestures.arraySize > 0)
            {
                EditorGUILayout.BeginVertical(EditorStyles.helpBox);

                string gestureNames = "Child Gestures: ";

                for (int i = 0; i < childGestures.arraySize; i++)
                {
                    if (childGestures.GetArrayElementAtIndex(i).objectReferenceValue != null)
                    {
                        string typeName = childGestures.GetArrayElementAtIndex(i).objectReferenceValue.GetType().ToString();
                        gestureNames += $"{typeName.Substring(typeName.LastIndexOf('.') + 1)} (";

                        SerializedObject parent = new SerializedObject(childGestures.GetArrayElementAtIndex(i).objectReferenceValue);
                        SerializedProperty childOrder = parent.FindProperty("_gestureOrder");

                        for (int j = 0; j < childOrder.arraySize; j++)
                        {
                            gestureNames += childOrder.GetArrayElementAtIndex(j).intValue + 1;
                            if (j < childOrder.arraySize - 1)
                            {
                                gestureNames += ", ";
                            }
                        }
                        gestureNames += ")";

                        if (i < childGestures.arraySize - 1)
                        {
                            gestureNames += ", ";
                        }
                    }
                }

                EditorGUILayout.LabelField(gestureNames, _wordWrap);
                EditorGUILayout.EndVertical();
            }

            DrawDefaultInspector();
        }
    }
}