using UnityEditor;
using UnityEngine;

namespace AiNav
{
    [CustomEditor(typeof(SharedMeshDb))]
    public class SharedMeshDbEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            DrawDefaultInspector();

            SharedMeshDb db = (SharedMeshDb)target;

            if (GUILayout.Button("Add Mesh"))
            {
                db.AddMesh();
            }

            if (GUILayout.Button("Save"))
            {
                db.Save();
            }

            if (GUILayout.Button("AddPrimitives"))
            {
                db.AddPrimitives();
            }

        }
    }
}
