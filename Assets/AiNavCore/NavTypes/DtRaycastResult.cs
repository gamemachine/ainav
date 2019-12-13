using System;
using Unity.Mathematics;

namespace AiNav
{
    [Serializable]
    public struct DtRaycastResult
    {
        public bool Hit;
        public float3 Position;
        public float3 Normal;
    }
}
