using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(Information))]
public class InformationEditor : Editor
{
    private bool editing = true;
    
    public override void OnInspectorGUI()
    {
        Information information = (Information) target;

        if (!editing)
        {
            EditorGUILayout.HelpBox(information.text, MessageType.Info);
            if (GUILayout.Button("Edit"))
            {
                editing = true;
            }
        }
        else
        {
            EditorGUILayout.LabelField("Information");
            information.text = EditorGUILayout.TextArea(information.text, GUILayout.Height(EditorGUIUtility.singleLineHeight * 4));
            if (GUILayout.Button("Save"))
            {
                editing = false;
            }
        }


    }
}