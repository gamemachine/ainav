using Unity.Mathematics;
using UnityEngine;

namespace AiNav
{
    public class BoxFilterAuthoring : MonoBehaviour
    {
        public int SurfaceId = 1;

        [SerializeField]
        private BoxCollider Collider;
        [SerializeField]
        private Collider TestPoint;

        public BoxFilter BoxFilter
        {
            get
            {
                BoxFilter filter = new BoxFilter();
                Bounds bounds = Collider.bounds;
                filter.Bounds = new DtBoundingBox(bounds.min, bounds.max);
                
                WorldBoxFilter box = new WorldBoxFilter();
                box.Size = Collider.size;
                box.Center = Collider.center;
                box.TRS = float4x4.TRS(transform.position, transform.rotation, transform.localScale);

                filter.WorldBoxFilter = box;
                return filter;
            }
        }

      
    }
}
