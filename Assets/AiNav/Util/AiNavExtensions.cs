using Unity.Entities.Serialization;
using Unity.Mathematics;
using UnityEngine;

namespace AiNav
{
    public static unsafe class AiNavExtensions
    {
        public static float3 WorldToLocal(this float4x4 transform, float3 point)
        {
            return math.transform(math.inverse(transform), point);
        }

        public static float3 LocalToWorld(this float4x4 transform, float3 point)
        {
            return math.transform(transform, point);
        }

        public static float3 WorldToLocal(this RigidTransform transform, float3 point)
        {
            return math.transform(math.inverse(transform), point);
        }

        public static float3 LocalToWorld(this RigidTransform transform, float3 point)
        {
            return math.transform(transform, point);
        }

        public static RigidTransform ToRigidTransform(this Transform transform)
        {
            return new RigidTransform(transform.rotation, transform.position);
        }

        public static int ReadInt(this BinaryReader reader)
        {
            int value;
            reader.ReadBytes(&value, sizeof(int));
            return value;
        }

        public static void Write(this BinaryWriter writer, int value)
        {
            writer.WriteBytes(&value, sizeof(int));
        }

    }
}
