using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Mathematics;

namespace AiNav
{
    public struct NavMeshNativeInputBuilder
    {
        public int2 Coord;
        public DtBoundingBox BoundingBox;
        public NativeList<float3> Vertices;
        public NativeList<int> Indices;
        public NativeList<byte> Areas;

        public NavMeshNativeInputBuilder(NavMeshTileBounds tileBounds)
        {
            Coord = tileBounds.Coord;
            BoundingBox = tileBounds.Bounds;
            Vertices = new NativeList<float3>(Allocator.TempJob);
            Indices = new NativeList<int>(Allocator.TempJob);
            Areas = new NativeList<byte>(Allocator.TempJob);
        }

        public void Dispose()
        {
            if (Vertices.IsCreated) Vertices.Dispose();
            if (Indices.IsCreated) Indices.Dispose();
            if (Areas.IsCreated) Areas.Dispose();
        }

        public unsafe NavMeshBuildInput ToBuildInput()
        {
            NavMeshBuildInput input = new NavMeshBuildInput();
            input.TileBounds = new NavMeshTileBounds(Coord, BoundingBox);

            input.Vertices = (float3*)Vertices.GetUnsafePtr();
            input.VerticesLength = Vertices.Length;

            input.Indices = (int*)Indices.GetUnsafePtr();
            input.IndicesLength = Indices.Length;

            input.Areas = (byte*)Areas.GetUnsafePtr();
            input.AreasLength = Areas.Length;

            return input;
        }

        public void Append(NavMeshNativeInputBuilder other)
        {
            // Copy vertices
            int vbase = Vertices.Length;
            for (int i = 0; i < other.Vertices.Length; i++)
            {
                float3 point = other.Vertices[i];
                Vertices.Add(point);
                BoundingBox = DtBoundingBox.Merge(BoundingBox, point);
            }

            // Copy indices with offset applied
            for (int i = 0; i < other.Indices.Length; i++)
            {
                Indices.Add(other.Indices[i] + vbase);
            }
        }

        public void Append(NativeArray<float3> vertices, NativeArray<int> indices, byte area = DtArea.WALKABLE)
        {
            // Copy vertices
            int vbase = Vertices.Length;
            for (int i = 0; i < vertices.Length; i++)
            {
                Vertices.Add(vertices[i]);
                BoundingBox = DtBoundingBox.Merge(BoundingBox, vertices[i]);
            }

            // Copy indices with offset applied
            for (int i = 0; i < indices.Length; i++)
            {
                Indices.Add(indices[i] + vbase);
            }

            int triangleCount = indices.Length / 3;
            for (int i = 0; i < triangleCount; i++)
            {
                Areas.Add(area);
            }
        }

        public void Append(NativeArray<float3> vertices, NativeArray<int> indices, NativeArray<byte> areas)
        {
            // Copy vertices
            int vbase = Vertices.Length;
            for (int i = 0; i < vertices.Length; i++)
            {
                Vertices.Add(vertices[i]);
                BoundingBox = DtBoundingBox.Merge(BoundingBox, vertices[i]);
            }

            // Copy indices with offset applied
            for (int i = 0; i < indices.Length; i++)
            {
                Indices.Add(indices[i] + vbase);
            }

            for (int i = 0; i < areas.Length; i++)
            {
                Areas.Add(areas[i]);
            }
        }
    }
}
