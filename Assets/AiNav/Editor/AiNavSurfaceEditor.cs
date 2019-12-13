using Unity.Entities;
using UnityEditor;
using UnityEngine;

namespace AiNav
{
    [CustomEditor(typeof(AiNavSurface))]
    public class AiNavSurfaceEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            DrawDefaultInspector();
            AiNavSurface surface = (AiNavSurface)target;
            int surfaceId = surface.GetId();

            if (!surface.enabled) return;

            var navSystem = surface.NavSystem;
            var controller = surface.Controller;
            

            if (navSystem != null)
            {
                if (controller != null)
                {
                    if (controller.Building)
                    {
                        if (GUILayout.Button("Stop Build"))
                        {
                            surface.StopBuild();
                        }
                    }
                }
               

                if ( (controller != null && !controller.Building) || controller == null)
                {
                    if (GUILayout.Button("Build"))
                    {
                        surface.BuildSurface();
                    }
                }

                bool surfaceVisible = surface.SurfaceVisible();

                if (NavMeshStoreSystem.Instance.HasTiles(surfaceId))
                {
                    if (surfaceVisible)
                    {
                        if (GUILayout.Button("HideSurface"))
                        {
                            surface.HideSurface();
                        }
                    }
                    else
                    {
                        if (GUILayout.Button("ShowSurface"))
                        {
                            surface.ShowSurface();
                        }
                    }

                }
                else
                {
                    if (surfaceVisible)
                    {
                        surface.HideSurface();
                    }
                }
            }
           
            


            if (GUILayout.Button("SaveSourcesAndFilters"))
            {
                surface.SaveSourcesAndFilters();
            }

            if (GUILayout.Button("DestroySurfaceData"))
            {
                surface.DestroySurfaceData();
            }

            if (GUILayout.Button("SetDefaultSettings"))
            {
                surface.SetDefaultSettings();
            }
            
        }
    }
}
