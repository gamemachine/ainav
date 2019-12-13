using Unity.Mathematics;

namespace AiNav
{
    public static class NavExtensions
    {
        public const float Epsilon = 1.17549435E-38f;

        public static bool Approximately(float a, float b)
        {
            return math.abs(b - a) < math.max(0.000001f * math.max(math.abs(a), math.abs(b)), Epsilon * 8);
        }

        public static bool Approximately(this float3 self, float3 other)
        {
            return Approximately(self.x, other.x) && Approximately(self.y, other.y) && Approximately(self.z, other.z);
        }

        public static bool Approximately(this DtBoundingBox self, DtBoundingBox other)
        {
            return Approximately(self.min, other.min) && Approximately(self.max, other.max);
        }
    }
}
