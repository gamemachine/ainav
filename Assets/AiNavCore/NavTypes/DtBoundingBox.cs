using System;
using System.Globalization;
using Unity.Mathematics;
using UnityEngine;

namespace AiNav
{
    [Serializable]
    public struct DtBoundingBox
    {
        public float3 min;
        public float3 max;

        public bool IsValid => math.all(min <= max);

        public static DtBoundingBox FromCenter(float3 center, float3 size)
        {
            float3 extents = size * 0.5f;
            return new DtBoundingBox(center - extents, center + extents);
        }

        public static DtBoundingBox FromUnityBounds(Bounds bounds)
        {
            return new DtBoundingBox(new float3(bounds.min.x, bounds.min.y, bounds.min.z), new float3(bounds.max.x, bounds.max.y, bounds.max.z));
        }

        public static DtBoundingBox Merge(DtBoundingBox value1, float3 value2)
        {
            DtBoundingBox result;
            result.min = math.min(value1.min, value2);
            result.max = math.max(value1.max, value2);
            return result;
        }

        public static DtBoundingBox Merge(DtBoundingBox value1, DtBoundingBox value2)
        {
            DtBoundingBox box;
            box.min = math.min(value1.min, value2.min);
            box.max = math.max(value1.max, value2.max);
            return box;
        }

        public static bool Intersects(ref DtBoundingBox box1, ref DtBoundingBox box2)
        {
            if (box1.min.x > box2.max.x || box2.min.x > box1.max.x)
                return false;

            if (box1.min.y > box2.max.y || box2.min.y > box1.max.y)
                return false;

            if (box1.min.z > box2.max.z || box2.min.z > box1.max.z)
                return false;

            return true;
        }

        public static bool ContainsPoint(ref DtBoundingBox box, ref float3 point)
        {
            if (box.min.x <= point.x && box.max.x >= point.x &&
                box.min.y <= point.y && box.max.y >= point.y &&
                box.min.z <= point.z && box.max.z >= point.z)
            {
                return true;
            }

            return false;
        }

        public Bounds ToUnityBounds()
        {
            Bounds bounds = default;
            bounds.min = new Vector3(min.x, min.y, min.z);
            bounds.max = new Vector3(max.x, max.y, max.z); ;
            return bounds;
        }

        public DtBoundingBox(float3 min, float3 max)
        {
            this.min = min;
            this.max = max;
        }

        public float3 Center
        {
            get { return (min + max) / 2; }
        }


        public float3 Extent
        {
            get { return (max - min) / 2; }
        }

        public void Expand(float amount)
        {
            min = new float3(min.x - amount, min.y - amount, min.z - amount);
            max = new float3(max.x + amount, max.y + amount, max.z + amount);
        }

        public bool ContainsPoint(float3 point)
        {
            return ContainsPoint(ref this, ref point);
        }

        public bool Intersects(DtBoundingBox other)
        {
            return Intersects(ref this, ref other);
        }

        public override string ToString()
        {
            return string.Format(CultureInfo.CurrentCulture, "Minimum:{0} Maximum:{1}", min.ToString(), max.ToString());
        }
    }
}
