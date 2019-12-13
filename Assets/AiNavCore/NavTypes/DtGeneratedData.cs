using System;

namespace AiNav
{
    [Serializable]
    public struct DtGeneratedData
    {
        public bool Success;
        public int Error;
        public IntPtr NavmeshVertices;
        public int NumNavmeshVertices;
        public IntPtr NavmeshData;
        public int NavmeshDataLength;
    }
}
