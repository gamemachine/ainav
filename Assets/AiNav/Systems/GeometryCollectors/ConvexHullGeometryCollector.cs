using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Mathematics;
using Unity.Physics;

namespace AiNav
{
    public unsafe struct ConvexHullGeometryCollector
    {
        public NavMeshNativeInputBuilder InputBuilder;
        [NativeDisableUnsafePtrRestriction]
        public ConvexCollider* Collider;
        public RigidTransform Transform;
        public DtBoundingBox Bounds;

        public ConvexHullGeometryCollector(NavMeshNativeInputBuilder inputBuilder, ConvexCollider* collider, RigidTransform transform, DtBoundingBox bounds)
        {
            InputBuilder = inputBuilder;
            Collider = collider;
            Transform = transform;
            Bounds = bounds;
        }

        public unsafe void Collect()
        {
            ConvexHull hull = Collider->ConvexHull;

            int totalNumVertices = 0;
            for (int f = 0; f < hull.NumFaces; f++)
            {
                totalNumVertices += hull.Faces[f].NumVertices + 1;
            }

            NativeArray<float3> vertices = new NativeArray<float3>(totalNumVertices, Allocator.Temp);
            NativeArray<int> triangles = new NativeArray<int>((totalNumVertices - hull.NumFaces) * 3, Allocator.Temp);

            int startVertexIndex = 0;
            int curTri = 0;
            for (int f = 0; f < hull.NumFaces; f++)
            {
                float3 avgFace = float3.zero;

                for (int fv = 0; fv < hull.Faces[f].NumVertices; fv++)
                {
                    int origV = hull.FaceVertexIndices[hull.Faces[f].FirstIndex + fv];
                    vertices[startVertexIndex + fv] = hull.Vertices[origV];

                    float3 v = hull.Vertices[origV];
                    avgFace += v;

                    triangles[curTri * 3 + 0] = startVertexIndex + fv;
                    triangles[curTri * 3 + 1] = startVertexIndex + (fv + 1) % hull.Faces[f].NumVertices;
                    triangles[curTri * 3 + 2] = startVertexIndex + hull.Faces[f].NumVertices;
                    curTri++;
                }
                avgFace *= 1.0f / hull.Faces[f].NumVertices;
                vertices[startVertexIndex + hull.Faces[f].NumVertices] = avgFace;

                startVertexIndex += hull.Faces[f].NumVertices + 1;
            }

            InputBuilder.Append(vertices, triangles);

            vertices.Dispose();
            triangles.Dispose();
        }

    }
}
