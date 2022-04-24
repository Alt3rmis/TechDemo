using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomEditor (typeof (WaypointsGenerator))]
public class WaypointsGeneratorEditor : Editor
{
    public override void OnInspectorGUI()
    {
        WaypointsGenerator wg = (WaypointsGenerator)target;
        DrawDefaultInspector();
        if (GUILayout.Button("Clear")){
            wg.ClearWaypoints();
        }
    }
}