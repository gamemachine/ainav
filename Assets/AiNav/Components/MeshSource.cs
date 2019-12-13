using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

namespace AiNav
{
    public unsafe struct MeshSource : IComponentData
    {
        public int Id;
        public MeshSourceInfo Info;
        public BlobAssetReference<MeshSourceData> Value;

        public static MeshSource Create(float4x4 localToWorld, Mesh mesh, int layer, byte area, byte flag, int customData = 0)
        {
            MeshSource source = new MeshSource();
            MeshSourceInfo info = new MeshSourceInfo {Layer = layer, Area = area, Flag = flag, CustomData = customData };

            source.Value = MeshSourceData.Create(mesh, localToWorld);
            info.Bounds = MeshSourceData.CalculateBounds(source.Value);

            source.Info = info;

            return source;
        }

        public static MeshSource CreateShared(float4x4 localToWorld, BlobAssetReference<MeshSourceData> sharedData, int layer, byte area, byte flag, int sharedMeshId, int customData = 0)
        {
            MeshSourceInfo info = new MeshSourceInfo { Layer = layer, Area = area, Flag = flag, CustomData = customData };

            info.Bounds = MeshSourceData.CalculateBounds(localToWorld, sharedData);
            info.TRS = localToWorld;
            info.SharedMeshId = sharedMeshId;

            return new MeshSource { Info = info };
        }

        public void Dispose()
        {
            if (!Info.Shared)
            {
                Value.Dispose();
            }
        }
    }

    public struct MeshSourceInfo
    {
        public int Layer;
        public byte Area;
        public byte Flag;
        public DtBoundingBox Bounds;
        public float4x4 TRS;
        public int SharedMeshId;
        public int CustomData;

        public bool Shared
        {
            get
            {
                return SharedMeshId > 0;
            }
        }
    }

    public unsafe struct MeshSourceData
    {
        public BlobArray<float3> Vertices;
        public BlobArray<int> Indices;

        public static unsafe BlobAssetReference<MeshSourceData> Create(NativeArray<int> indices, NativeArray<float3> vertices)
        {
            var builder = new BlobBuilder(Allocator.Temp);
            ref var data = ref builder.ConstructRoot<MeshSourceData>();
            
            BlobBuilderArray<float3> vertData = builder.Allocate(ref data.Vertices, vertices.Length);

            for (int i = 0; i < vertices.Length; i++)
            {
                vertData[i] = vertices[i];
            }

            BlobBuilderArray<int> indiceData = builder.Allocate(ref data.Indices, indices.Length);

            for (int i = 0; i < indices.Length; i++)
            {
                indiceData[i] = indices[i];
            }

            var blob = builder.CreateBlobAssetReference<MeshSourceData>(Allocator.Persistent);
            builder.Dispose();
            return blob;
        }

        public static BlobAssetReference<MeshSourceData> Create(Mesh mesh, float4x4 localToWorld)
        {
            MeshToArrays(mesh, out NativeArray<int> indices, out NativeArray<float3> vertices);
            var transformed = Transform(localToWorld, vertices);
            vertices.Dispose();

            BlobAssetReference<MeshSourceData> blob = Create(indices, transformed);
            indices.Dispose();
            transformed.Dispose();
            return blob;
        }

        public static BlobAssetReference<MeshSourceData> Create(Mesh mesh)
        {
            MeshToArrays(mesh, out NativeArray<int> indices, out NativeArray<float3> vertices);

            BlobAssetReference<MeshSourceData> blob = Create(indices, vertices);
            indices.Dispose();
            vertices.Dispose();
            return blob;
        }

        public static Mesh CreateMesh(PrimitiveType type)
        {
            GameObject gameObject = GameObject.CreatePrimitive(type);
            Mesh mesh = gameObject.GetComponent<MeshFilter>().sharedMesh;
            Object.DestroyImmediate(gameObject);
            return mesh;
        }

        public static BlobAssetReference<MeshSourceData> Create(PrimitiveType type)
        {
            GameObject gameObject = GameObject.CreatePrimitive(type);
            Mesh mesh = gameObject.GetComponent<MeshFilter>().sharedMesh;
            Object.DestroyImmediate(gameObject);
            return Create(mesh);
        }


        public static void GetDataSafe(BlobAssetReference<MeshSourceData> data, out NativeArray<int> indices, out NativeArray<float3> vertices)
        {
            ref var source = ref data.Value;

            indices = new NativeArray<int>(source.Indices.Length, Allocator.Temp);
            for (int i = 0; i < indices.Length; i++)
            {
                indices[i] = source.Indices[i];
            }

            vertices = new NativeArray<float3>(source.Vertices.Length, Allocator.Temp);
            for (int i = 0; i < vertices.Length; i++)
            {
                vertices[i] = source.Vertices[i];
            }
        }

        public static unsafe void GetData(BlobAssetReference<MeshSourceData> data, out NativeArray<int> indices, out NativeArray<float3> vertices)
        {
            ref var source = ref data.Value;

            indices = new NativeArray<int>(source.Indices.Length, Allocator.Temp);
            UnsafeUtility.MemCpy(indices.GetUnsafePtr(), source.Indices.GetUnsafePtr(), indices.Length * UnsafeUtility.SizeOf<int>());

            vertices = new NativeArray<float3>(source.Vertices.Length, Allocator.Temp);
            UnsafeUtility.MemCpy(vertices.GetUnsafePtr(), source.Vertices.GetUnsafePtr(), vertices.Length * UnsafeUtility.SizeOf<float3>());
        }

        public static unsafe BlobAssetReference<MeshSourceData> Transform(BlobAssetReference<MeshSourceData> data, float4x4 matrix)
        {
            GetData(data, out NativeArray<int> indices, out NativeArray<float3> vertices);
            var transformed = Transform(matrix, vertices);
            vertices.Dispose();

            BlobAssetReference<MeshSourceData> blob = Create(indices, transformed);
            indices.Dispose();
            transformed.Dispose();
            return blob;
        }

        public static void MeshToArrays(Mesh mesh, out NativeArray<int> outIndices, out NativeArray<float3> outVertices)
        {
            Vector3[] vertices = mesh.vertices;
            int[] indices = mesh.triangles;

            outVertices = new NativeArray<float3>(vertices.Length, Allocator.Temp);
            for (int i = 0; i < vertices.Length; i++)
            {
                outVertices[i] = vertices[i];
            }

            outIndices = new NativeArray<int>(indices.Length, Allocator.Temp);
            outIndices.CopyFrom(indices);
        }

        public static NativeArray<float3> Transform(float4x4 matrix, NativeArray<float3> vertices)
        {
            NativeArray<float3> transformed = new NativeArray<float3>(vertices.Length, Allocator.Temp);
            for (int i = 0; i < vertices.Length; i++)
            {
                transformed[i] = math.transform(matrix,vertices[i]);
            }
            return transformed;
        }

        public static void TransformInPlace(float4x4 matrix, NativeArray<float3> vertices)
        {
            for (int i = 0; i < vertices.Length; i++)
            {
                vertices[i] = math.transform(matrix, vertices[i]);
            }
        }

        public static DtBoundingBox CalculateBounds(BlobAssetReference<MeshSourceData> data)
        {
            ref var source = ref data.Value;
            DtBoundingBox bounds = default;
            for (int i = 0; i < source.Vertices.Length; i++)
            {
                float3 point = source.Vertices[i];
                if (i == 0)
                {
                    bounds = new DtBoundingBox(point, point);
                }
                else
                {
                    bounds = DtBoundingBox.Merge(bounds, point);
                }
            }
            return bounds;
        }

        public static DtBoundingBox CalculateBounds(float4x4 matrix, BlobAssetReference<MeshSourceData> data)
        {
            ref var source = ref data.Value;
            DtBoundingBox bounds = default;
            for (int i = 0; i < source.Vertices.Length; i++)
            {
                float3 point = math.transform(matrix, source.Vertices[i]);
                if (i == 0)
                {
                    bounds = new DtBoundingBox(point, point);
                } else
                {
                    bounds = DtBoundingBox.Merge(bounds, point);
                }
            }
            return bounds;
        }
    }

}
