using Unity.Collections.LowLevel.Unsafe;
using Unity.Mathematics;

namespace AiNav
{
    public unsafe struct NavMeshBuildInput
    {
        public NavMeshTileBounds TileBounds;

        [NativeDisableUnsafePtrRestriction]
        public float3* Vertices;
        public int VerticesLength;

        [NativeDisableUnsafePtrRestriction]
        public int* Indices;
        public int IndicesLength;

        [NativeDisableUnsafePtrRestriction]
        public byte* Areas;
        public int AreasLength;
    }
}
