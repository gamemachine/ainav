using Unity.Entities;
using UnityEditor;
using UnityEngine;

namespace AiNav
{
    [CustomEditor(typeof(EcsWorld))]
    public class EcsWorldEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            DrawDefaultInspector();

            if (!EditorApplication.isPlaying)
            {
                if (EcsWorld.EditorModeEnabled)
                {
                    GUILayout.Label("Ecs is running");

                    if (GUILayout.Button("StopEditorSystems"))
                    {
                        EcsWorld.StopEditorSystems();
                    }
                }
            }
            
        }
    }
}
