using System;

namespace AiNav
{
    [Serializable]
    public struct GeometryFilter
    {
        public bool ApplyWaterFilter;
        public float WaterLevel;
        public float WaterSpan;

        public bool ApplyMinHeight;
        public float MinHeight;

        public bool FilterHeight(float height)
        {
            return (ApplyMinHeight && height < MinHeight);
        }

        public bool FilterWater(float height)
        {
            if (!ApplyWaterFilter) return false;
            float min = WaterLevel - WaterSpan;
            float max = WaterLevel + WaterSpan;
            return (height >= min && height <= max);
        }
    }
}
