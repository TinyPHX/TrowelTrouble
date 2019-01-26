using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(CameraShake))]
[ExecuteInEditMode]
public class CameraShakeEditor : Editor
{   
    public override void OnInspectorGUI()
    {
        CameraShake cameraShake = (CameraShake)target;

        DrawDefaultInspector();

        if (GUILayout.Button("Trigger Shake")) { cameraShake.Shake(); }
    }
}
