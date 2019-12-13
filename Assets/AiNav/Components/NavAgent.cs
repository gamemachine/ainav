using Unity.Entities;
using Unity.Mathematics;

namespace AiNav
{
    [System.Serializable]
    public struct NavAgent : IComponentData
    {
        public bool Active;
        public int CrowdIndex;
        public int UpdateParams;
        public DtAgentParams DtAgentParams;
        public DtCrowdAgent DtCrowdAgent;
        public float3 Destination;
        public bool SetDestination;
        public bool SetDestinationFailed;

        public bool ClosestRandomPointOnCircle(AiNavQuery query, ref Random random, float range, out float3 closest)
        {
            float2 current = new float2(DtCrowdAgent.Position.x, DtCrowdAgent.Position.z);
            float2 pos = RandomPointInCircle(current, range, ref random);
            float3 target = new float3(pos.x, DtCrowdAgent.Position.y, pos.y);
            float3 extent = new float3(range, range, range);
            return query.SamplePosition(target, extent, out closest);
        }

        public static float2 RandomPointInCircle(float2 center, float radius, ref Random rand)
        {
            float x = rand.NextFloat(center.x - radius, center.x + radius);
            float y = rand.NextFloat(center.y - radius, center.y + radius);
            return new float2(x, y);
        }
    }

}
