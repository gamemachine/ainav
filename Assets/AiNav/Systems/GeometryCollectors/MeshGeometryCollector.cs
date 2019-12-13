using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Mathematics;
using Unity.Physics;

namespace AiNav
{
    public unsafe struct MeshGeometryCollector
    {
        public NavMeshNativeInputBuilder InputBuilder;
        [NativeDisableUnsafePtrRestriction]
        public MeshCollider* Collider;
        public RigidTransform Transform;
        public DtBoundingBox Bounds;

        public MeshGeometryCollector(NavMeshNativeInputBuilder inputBuilder, MeshCollider* Collider, RigidTransform transform, DtBoundingBox bounds)
        {
            InputBuilder = inputBuilder;
            this.Collider = Collider;
            Transform = transform;
            Bounds = bounds;
        }

        private bool HasFlag(Mesh.PrimitiveFlags self, Mesh.PrimitiveFlags flag)
        {
            return (self & flag) == flag;
        }

        public unsafe void Collect()
        {
            int vertexIndex = 0;
            ref Mesh mesh = ref Collider->Mesh;

            NativeList<float3> vertices = new NativeList<float3>(Allocator.Temp);
            NativeList<int> triangles = new NativeList<int>(Allocator.Temp);

            for (int sectionIndex = 0; sectionIndex < mesh.Sections.Length; sectionIndex++)
            {
                ref Mesh.Section section = ref mesh.Sections[sectionIndex];
                for (int primitiveIndex = 0; primitiveIndex < section.PrimitiveVertexIndices.Length; primitiveIndex++)
                {
                    Mesh.PrimitiveVertexIndices vertexIndices = section.PrimitiveVertexIndices[primitiveIndex];
                    Mesh.PrimitiveFlags flags = section.PrimitiveFlags[primitiveIndex];
                    int numTriangles = HasFlag(flags, Mesh.PrimitiveFlags.IsTrianglePair) ? 2 : 1;

                    float3x4 v = new float3x4(
                        section.Vertices[vertexIndices.A],
                        section.Vertices[vertexIndices.B],
                        section.Vertices[vertexIndices.C],
                        section.Vertices[vertexIndices.D]);

                    

                    for (int triangleIndex = 0; triangleIndex < numTriangles; triangleIndex++)
                    {
                        float3 a = v[0];
                        float3 b = v[1 + triangleIndex];
                        float3 c = v[2 + triangleIndex];

                        //a = localToWorldMatrix.MultiplyPoint3x4(a);
                        //b = localToWorldMatrix.MultiplyPoint3x4(b);
                        //c = localToWorldMatrix.MultiplyPoint3x4(c);

                        a = math.transform(Transform, a);
                        b = math.transform(Transform, b);
                        c = math.transform(Transform, c);

                        if (!Bounds.ContainsPoint(a) && !Bounds.ContainsPoint(b) && !Bounds.ContainsPoint(c))
                        {
                            //continue;
                        }

                        vertices.Add(a);
                        vertices.Add(b);
                        vertices.Add(c);

                        triangles.Add(vertexIndex++);
                        triangles.Add(vertexIndex++);
                        triangles.Add(vertexIndex++);
                    }
                }
            }

            InputBuilder.Append(vertices, triangles);

            vertices.Dispose();
            triangles.Dispose();
        }
    }
}
