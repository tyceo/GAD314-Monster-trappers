// using UnityEngine;
// using UnityEditor;
// [CustomEditor(typeof(AStar))]
// public class AStarEditor : Editor
// {
//     public override void OnInspectorGUI()
//     {
//         DrawDefaultInspector();
//
//         var pathfinder = (AStar)target;
//
//         GUI.enabled = pathfinder && pathfinder.grid && pathfinder.start && pathfinder.target;
//         if (GUILayout.Button("Run AStar immediately"))
//         {
//             pathfinder.RunAStarImmediately();
//             SceneView.RepaintAll();
//         }
//         if (GUILayout.Button("Run AStar gradually"))
//         {
//             pathfinder.RunAStarGradually();
//             SceneView.RepaintAll();
//         }
//
//         if (GUILayout.Button("Stop AStar Coroutine"))
//         {
//             pathfinder.StopCoroutineIfRunning();
//             SceneView.RepaintAll();
//         }
//         GUI.enabled = true;
//     }
// }
