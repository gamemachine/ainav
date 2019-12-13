using UnityEditor;
using UnityEngine;

namespace AiNav
{
    [CustomEditor(typeof(SimpleMeshSourceAuthor))]
    public class SimpleSourceAuthorEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            DrawDefaultInspector();

            SimpleMeshSourceAuthor author = (SimpleMeshSourceAuthor)target;

            if (GUILayout.Button("Add To Surface"))
            {
                author.AddToSurface();
            }

            if (author.Current.Id > 0 && GUILayout.Button("Remove from Surface"))
            {
                author.RemoveFromSurface();
            }
        }
    }
}