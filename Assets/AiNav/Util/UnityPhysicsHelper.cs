using Unity.Physics;
using UnityEngine;

namespace AiNav.Util
{
    public static class UnityPhysicsHelper
    {
        public static bool HasLayer(this CollisionFilter filter, int layer)
        {
            uint mask = 1u << layer;
            return (filter.BelongsTo & mask) == mask;
        }

        public static CollisionFilter LayerMaskToFilter(LayerMask mask)
        {
            CollisionFilter filter = new CollisionFilter()
            {
                BelongsTo = (uint)mask.value,
                CollidesWith = (uint)mask.value
            };
            return filter;
        }


        
    }
}
