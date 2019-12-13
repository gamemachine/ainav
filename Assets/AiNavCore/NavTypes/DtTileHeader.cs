using System;

namespace AiNav
{
    [Serializable]
    public unsafe struct DtTileHeader
    {
        public int Magic;
        public int Version;
        public int X;
        public int Y;
        public int Layer;
        public uint UserId;
        public int PolyCount;
        public int VertCount;
        public int MaxLinkCount;
        public int DetailMeshCount;
        public int DetailVertCount;
        public int DetailTriCount;
        public int BvNodeCount;
        public int OffMeshConCount;
        public int OffMeshBase;
        public float WalkableHeight;
        public float WalkableRadius;
        public float WalkableClimb;
        public fixed float Bmin[3];
        public fixed float Bmax[3];
        public float BvQuantFactor;
    }
}
