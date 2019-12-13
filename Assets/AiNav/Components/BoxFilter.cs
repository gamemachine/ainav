using Unity.Entities;
using Unity.Mathematics;

namespace AiNav
{
    public struct BoxFilter : IComponentData
    {
        public DtBoundingBox Bounds;
        public WorldBoxFilter WorldBoxFilter;
        public bool Active;
    }

    public struct WorldBoxFilter
    {
        public float3 Center;
        public float3 Size;
        public float4x4 TRS;


        public bool ContainsPoint(float3 point)
        {
            point = TRS.WorldToLocal(point) - Center;

            float halfX = (Size.x * 0.5f);
            float halfY = (Size.y * 0.5f);
            float halfZ = (Size.z * 0.5f);
            if (point.x < halfX && point.x > -halfX && point.y < halfY && point.y > -halfY && point.z < halfZ && point.z > -halfZ)
            {
                return true;
            }
            else
            {
                return false;
            }
        }
    }
}
