using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Mathematics;

namespace AiNav
{
    public unsafe struct PrimitiveGeometryCollector
    {
        public NavMeshNativeInputBuilder InputBuilder;
        [NativeDisableUnsafePtrRestriction]
        public Unity.Physics.TerrainCollider* TerrainCollider;
        public RigidTransform Transform;
        public DtBoundingBox Bounds;

        public PrimitiveGeometryCollector(NavMeshNativeInputBuilder inputBuilder, Unity.Physics.TerrainCollider* terrainCollider, RigidTransform transform, DtBoundingBox bounds)
        {
            InputBuilder = inputBuilder;
            TerrainCollider = terrainCollider;
            Transform = transform;
            Bounds = bounds;
        }

        public unsafe void Collect()
        {
            ref var terrain = ref TerrainCollider->Terrain;

            NativeList<float3> vertices = new NativeList<float3>(Allocator.Temp);
            NativeList<int> triangles = new NativeList<int>(Allocator.Temp);

            int vertexIndex = 0;
            for (int i = 0; i < terrain.Size.x - 1; i++)
            {
                for (int j = 0; j < terrain.Size.y - 1; j++)
                {
                    int i0 = i;
                    int i1 = i + 1;
                    int j0 = j;
                    int j1 = j + 1;
                    float3 v0 = new float3(i0, terrain.Heights[i0 + terrain.Size.x * j0], j0) * terrain.Scale;
                    float3 v1 = new float3(i1, terrain.Heights[i1 + terrain.Size.x * j0], j0) * terrain.Scale;
                    float3 v2 = new float3(i0, terrain.Heights[i0 + terrain.Size.x * j1], j1) * terrain.Scale;
                    float3 v3 = new float3(i1, terrain.Heights[i1 + terrain.Size.x * j1], j1) * terrain.Scale;

                    v0 = math.transform(Transform, v0);
                    v1 = math.transform(Transform, v1);
                    v2 = math.transform(Transform, v2);
                    v3 = math.transform(Transform, v3);

                    if (!Bounds.ContainsPoint(v0) && !Bounds.ContainsPoint(v1) && !Bounds.ContainsPoint(v2) && !Bounds.ContainsPoint(v3))
                    {
                        continue;
                    }

                    vertices.Add(v0);
                    vertices.Add(v1);
                    vertices.Add(v2);
                    vertices.Add(v3);

                    triangles.Add(vertexIndex + 1);
                    triangles.Add(vertexIndex);
                    triangles.Add(vertexIndex + 2);

                    triangles.Add(vertexIndex + 1);
                    triangles.Add(vertexIndex + 2);
                    triangles.Add(vertexIndex + 3);

                    vertexIndex += 4;
                }
            }

            InputBuilder.Append(vertices, triangles);

            vertices.Dispose();
            triangles.Dispose();
        }
    }
}
