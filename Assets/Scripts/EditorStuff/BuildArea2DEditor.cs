using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(BuildArea2D))]
public class BuildArea2DEditor : Editor
{
    public override void OnInspectorGUI()
    {
        BuildArea2D targetBuildArea = (BuildArea2D)target;

       
        if (GUILayout.Button("Generate"))
        {
            targetBuildArea.ResetBuild();
            BuildingGenerator BG = targetBuildArea.StartGenerate();
            BG.generate();

        }
        if (GUILayout.Button("Destroy"))
        {
            targetBuildArea.ResetBuild();
        }
        
        DrawDefaultInspector();

    }
      
}
