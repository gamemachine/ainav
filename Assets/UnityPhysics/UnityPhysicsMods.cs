using System;
using Unity.Collections;
using Unity.Mathematics;

namespace AiGame.UnityPhysics
{
    
    public class UnityPhysicsMods
    {
        
        public struct MappedVert
        {
            public float3 Vert;
            public int Index;
        }

        public static NativeArray<float3> WeldVertices(NativeArray<int> indices, NativeArray<float3> vertices)
        {
            bool hasDups = false;
            int uniqueVertIndex = 0;
            NativeHashMap<uint, MappedVert> uniqueVertMap = new NativeHashMap<uint, MappedVert>(vertices.Length, Allocator.Temp);
            for(int i=0;i<vertices.Length;i++)
            {
                float3 vert = vertices[i];
                uint hash = math.hash(vert);
                MappedVert mapped = new MappedVert { Vert = vert, Index = uniqueVertIndex };
                if (uniqueVertMap.TryAdd(hash, mapped))
                {
                    uniqueVertIndex++;
                } else
                {
                    hasDups = true;
                }
            }

            if (!hasDups)
            {
                return vertices;
            }

            NativeArray<MappedVert> values = uniqueVertMap.GetValueArray(Allocator.Temp);
            NativeArray<float3> uniqueVerts = new NativeArray<float3>(values.Length, Allocator.Temp);
            

            for(int i=0;i<values.Length;i++)
            {
                MappedVert mapped = values[i];
                uniqueVerts[mapped.Index] = mapped.Vert;
            }

            for (int i=0;i<indices.Length;i++)
            {
                int index = indices[i];
                float3 vert = vertices[index];
                uint hash = math.hash(vert);
                if (!uniqueVertMap.TryGetValue(hash, out MappedVert mapped))
                {
                    throw new ArgumentException("Vert not found");
                }

                indices[i] = mapped.Index;
            }

            return uniqueVerts;
        }
    }
}
