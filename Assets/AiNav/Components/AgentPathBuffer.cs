using Unity.Entities;
using Unity.Mathematics;

namespace AiNav
{

    [InternalBufferCapacity(16)]
    public struct AgentPathBuffer : IBufferElementData
    {
        public static implicit operator float3(AgentPathBuffer e) { return e.Value; }
        public static implicit operator AgentPathBuffer(float3 e) { return new AgentPathBuffer { Value = e }; }

        public float3 Value;
    }
}
