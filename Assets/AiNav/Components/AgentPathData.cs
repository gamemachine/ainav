using System;
using Unity.Entities;
using Unity.Mathematics;

namespace AiNav
{
    [Serializable]
    public struct AgentPathData : IComponentData
    {
        public int PathLength;
        public int CurrentIndex;

        public bool UsingPath;
        public int LastQueryResult;
        public float3 CurrentWaypoint;
        public bool HasPath;
        public float DistanceToDestination;
        public float DistanceToWaypoint;
    }
}
