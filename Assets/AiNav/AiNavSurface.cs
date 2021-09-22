using AiNav.Util;
using System.Collections.Generic;
using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

namespace AiNav
{
    public class AiNavSurface : MonoBehaviour
    {
        [SerializeField]
        private int SurfaceId;
        [SerializeField]
        private bool AutoUpdateVisuals = true;
        [SerializeField]
        public LayerMask IncludeMask;
        [SerializeField]
        public bool IncludeUnityPhysicsGeometry;
        [SerializeField]
        private int BatchSize = 40;
        [SerializeField]
        private float3 Center;
        [SerializeField]
        private float3 Size;
        [SerializeField]
        public GeometryFilter GeometryFilter;
        [SerializeField]
        private NavMeshBuildSettings BuildSettings;
        [SerializeField]
        private NavAgentSettings AgentSettings;
        [SerializeField]
        private bool CrowdEnabled;
        [SerializeField]
        private MeshFilter DisplayFilter;
        
        public SurfaceController Controller
        {
            get
            {
                var navSystem = NavSystem;
                if (navSystem == null) return null;
                return navSystem.GetSurfaceController(SurfaceId);
            }
        }

        public AiNavSystem NavSystem
        {
            get
            {
                if (EcsWorld.Active == null) return null;
                return EcsWorld.Active.GetExistingSystem<AiNavSystem>();
            }
        }

        private void OnEnable()
        {
            EnsureSurfaceAdded();
        }

        private void OnDisable()
        {
            if (EcsWorld.Active == null) return;
            var navSystem = EcsWorld.Active.GetExistingSystem<AiNavSystem>();
            if (navSystem != null)
            {
                navSystem.RemoveSurface(SurfaceId);
            }
        }

        public int GetId()
        {
            return SurfaceId;
        }

        public bool EnsureSurfaceAdded()
        {
            var navSystem = NavSystem;
            if (navSystem == null) return false;
            
            if (!navSystem.HasSurface(SurfaceId))
            {
                SurfaceControllerConfig config = new SurfaceControllerConfig
                {
                    SurfaceId = SurfaceId,
                    BuildSettings = BuildSettings,
                    AgentSettings = AgentSettings,
                    BatchSize = BatchSize,
                    CrowdEnabled = CrowdEnabled,
                    GeometryFilter = GeometryFilter,
                    IncludeMask = UnityPhysicsHelper.LayerMaskToFilter(IncludeMask),
                    IncludeUnityPhysicsGeometry = IncludeUnityPhysicsGeometry
                };
                navSystem.SetSurface(config);
            }
            return true;
        }

        public void SaveSourcesAndFilters()
        {
            SurfaceData.SaveScene(SurfaceId);
        }

        public void SetDefaultSettings()
        {
            BuildSettings = NavMeshBuildSettings.Default();
            AgentSettings = NavAgentSettings.Default();
        }

        public void BuildSurface()
        {
            if (!EnsureSurfaceAdded())
            {
                return;
            }

            Bounds bounds = new Bounds(Center, Size);
            DtBoundingBox box = DtBoundingBox.FromUnityBounds(bounds);

            var controller = Controller;
            controller.MarkDirty(box);
            Controller.OnBuildCompleted -= OnBuildCompleted;
            controller.OnBuildCompleted += OnBuildCompleted;
        }

        public void StopBuild()
        {
            var navSystem = NavSystem;
            if (navSystem != null)
            {
                var controller = Controller;
                if (controller != null)
                {
                    navSystem.RemoveSurface(SurfaceId);
                }
            }
        }

        public void DestroySurfaceData()
        {
            NavMeshStoreSystem.Instance.DestroySurfaceData(SurfaceId);
        }

        public void ShowSurface()
        {
            NavMeshTestData data = new NavMeshTestData();
            Dictionary<int2, NavMeshTile> tiles = new Dictionary<int2, NavMeshTile>();
            NavMeshStoreSystem.Instance.LoadTiles(SurfaceId, tiles);
            
            if (tiles.Count > 0)
            {
                data.SetTiles(tiles);
                DisplayFilter.sharedMesh = data.ToMesh();
                DisplayFilter.gameObject.SetActive(true);
            }
        }

        public bool SurfaceVisible()
        {
            return DisplayFilter.gameObject.activeSelf;
        }

        public void HideSurface()
        {
            if (DisplayFilter.gameObject.activeSelf)
            {
                DisplayFilter.sharedMesh = null;
                DisplayFilter.gameObject.SetActive(false);
            }
        }

        private void OnBuildCompleted(int surfaceId)
        {
            if (SurfaceId != surfaceId) return;
            if (AutoUpdateVisuals) ShowSurface();
        }

    }
}
