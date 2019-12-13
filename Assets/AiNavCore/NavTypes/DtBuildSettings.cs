using System;
using Unity.Mathematics;

namespace AiNav
{
    [Serializable]
    public struct DtBuildSettings
    {
        public DtBoundingBox BoundingBox;
        public float CellHeight;
        public float CellSize;
        public int TileSize;
        public int2 TilePosition;
        public int RegionMinArea;
        public int RegionMergeArea;
        public float EdgeMaxLen;
        public float EdgeMaxError;
        public float DetailSampleDist;
        public float DetailSampleMaxError;
        public float AgentHeight;
        public float AgentRadius;
        public float AgentMaxClimb;
        public float AgentMaxSlope;
    }
}
