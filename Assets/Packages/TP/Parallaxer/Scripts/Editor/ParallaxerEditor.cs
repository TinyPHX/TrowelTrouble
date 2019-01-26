using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace TP.Parallaxer
{
    [CustomEditor(typeof(ParallaxLayer)), ExecuteInEditMode, CanEditMultipleObjects]
    public class ParallaxerEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            DrawDefaultInspector();

            if (GUILayout.Button("Refresh"))
            {
                foreach (ParallaxLayer parallaxer in Array.ConvertAll(targets, item => (ParallaxLayer)item))
                {
                    parallaxer.Initialize();
                    parallaxer.UpdatePositions();
                }
            }
        }
    }
}
