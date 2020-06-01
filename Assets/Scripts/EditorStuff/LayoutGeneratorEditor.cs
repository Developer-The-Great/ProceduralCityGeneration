using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(CityLayoutGenerator))]
public class LayoutGeneratorEditor : Editor
{
    public override void OnInspectorGUI()
    {
        CityLayoutGenerator targetLG = (CityLayoutGenerator)target;


        if (GUILayout.Button("Generate"))
        {
            targetLG.ResetBuild();
            targetLG.Init();
            
            
        }
        if (GUILayout.Button("Destroy"))
        {
            targetLG.ResetBuild();
        }

        if (GUILayout.Button("Generate Buildings"))
        {
            targetLG.GenerateBuildings();

        }

        DrawDefaultInspector();

    }
}
