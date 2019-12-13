using AiNav.Collections;
using Unity.Mathematics;

namespace AiNav
{
    public class NavMeshInputBuilder
    {
        public int2 Coord;
        public DtBoundingBox BoundingBox;
        public AiNativeList<float3> Vertices;
        public AiNativeList<int> Indices;
        public AiNativeList<byte> Areas;

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
        
        public NavMeshInputBuilder(NavMeshTileBounds tileBounds)
        {
            Coord = tileBounds.Coord;
            BoundingBox = tileBounds.Bounds;
            Vertices = new AiNativeList<float3>(2);
            Indices = new AiNativeList<int>(2);
            Areas = new AiNativeList<byte>(2);
        }

        public void Dispose()
        {
            Vertices.Dispose();
            Indices.Dispose();
            Areas.Dispose();
        }

        public void Append(NavMeshInputBuilder other)
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

        public unsafe void Append(float3* vertices, int verticesLength, int* indices, int indicesLength, byte area = DtArea.WALKABLE)
        {
            // Copy vertices
            int vbase = Vertices.Length;
            for (int i = 0; i < verticesLength; i++)
            {
                Vertices.Add(vertices[i]);
                BoundingBox = DtBoundingBox.Merge(BoundingBox, vertices[i]);
            }

            // Copy indices with offset applied
            for (int i = 0; i < indicesLength; i++)
            {
                Indices.Add(indices[i] + vbase);
            }

            int triangleCount = indicesLength / 3;
            for (int i = 0; i < triangleCount; i++)
            {
                Areas.Add(area);
            }
        }

        public void Append(AiNativeList<float3> vertices, AiNativeList<int> indices, byte area = DtArea.WALKABLE)
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

        public void Append(float3[] vertices, int[] indices, byte area = DtArea.WALKABLE)
        {
            // Copy vertices
            int vbase = Vertices.Length;
            for (int i = 0; i < vertices.Length; i++)
            {
                Vertices.Add(vertices[i]);
                BoundingBox = DtBoundingBox.Merge(BoundingBox, vertices[i]);
            }

            // Copy indices with offset applied
            for(int i=0;i<indices.Length;i++)
            {
                Indices.Add(indices[i] + vbase);
            }

            int triangleCount = indices.Length / 3;
            for(int i=0;i<triangleCount;i++)
            {
                Areas.Add(area);
            }
        }

        public void Append(float3[] vertices, int[] indices, byte[] areas)
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
