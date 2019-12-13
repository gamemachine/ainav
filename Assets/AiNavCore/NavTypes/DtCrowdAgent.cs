using System;
using Unity.Entities;
using Unity.Mathematics;

namespace AiNav
{
    [Serializable]
    public struct DtCrowdAgent : IComponentData
    {
        public int Active;
        public int Partial;
        public float DesiredSpeed;
        public float3 Position;
        public float3 Velocity;
    }
}
