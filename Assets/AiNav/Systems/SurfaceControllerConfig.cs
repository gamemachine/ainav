using Unity.Physics;

namespace AiNav
{
    public struct SurfaceControllerConfig
    {
        public int SurfaceId;
        public NavMeshBuildSettings BuildSettings;
        public NavAgentSettings AgentSettings;
        public int BatchSize;
        public bool CrowdEnabled;
        public GeometryFilter GeometryFilter;
        public CollisionFilter IncludeMask;
        public bool IncludeUnityPhysicsGeometry;
    }
}
