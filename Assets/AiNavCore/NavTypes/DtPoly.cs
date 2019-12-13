using System;

namespace AiNav
{
    [Serializable]
    public unsafe struct DtPoly
    {
        public uint FirstLink;
        public fixed ushort Vertices[6];
        public fixed ushort Neighbours[6];
        public ushort Flags;
        public byte VertexCount;
        public byte AreaAndType;
    }
}
