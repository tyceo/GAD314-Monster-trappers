using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(AStar3D))]
public class AStar3DEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        var pathfinder = (AStar3D)target;

        GUI.enabled = pathfinder && pathfinder.grid && pathfinder.start && pathfinder.target;

        if (GUILayout.Button("Run AStar immediately"))
        {
            pathfinder.RunAStarImmediately();
            SceneView.RepaintAll();
        }

        if (GUILayout.Button("Run AStar gradually"))
        {
            pathfinder.RunAStarGradually();
            SceneView.RepaintAll();
        }

        if (GUILayout.Button("Stop AStar Coroutine"))
        {
            pathfinder.StopCoroutineIfRunning();
            SceneView.RepaintAll();
        }
        GUI.enabled = true;
    }
}