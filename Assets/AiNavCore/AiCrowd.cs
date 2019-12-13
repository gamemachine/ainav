using System;
using System.Collections.Generic;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Mathematics;

namespace AiNav
{
    public unsafe struct AiCrowd : IDisposable
    {
        [NativeDisableUnsafePtrRestriction]
        private IntPtr DtCrowd;
        [NativeDisableUnsafePtrRestriction]
        private IntPtr DtNavMesh;

        public AiCrowd(IntPtr navmesh, int maxAgents = 512, float maxRadius = 4f)
        {
            DtCrowd = Navigation.Crowd.Create(navmesh, maxAgents, maxRadius);
            if (DtCrowd == IntPtr.Zero)
            {
                throw new ApplicationException("Unable to create crowd");
            }
            DtNavMesh = navmesh;
        }

        public void Dispose()
        {
            if (DtCrowd != IntPtr.Zero)
            {
                Navigation.Crowd.Destroy(DtCrowd);
                DtCrowd = IntPtr.Zero;
            }
        }

        public int AddAgentAtRandomPosition(DtAgentParams agentParams, AiNavQuery query)
        {
            float3 onMesh = default;
            if (Navigation.Query.GetRandomPosition(DtNavMesh, ref onMesh) == 1)
            {
                return AddAgent(onMesh, agentParams);
            }
            return -1;
        }

        public bool TryAddAgent(float3 position, DtAgentParams agentParams, out int agentIndex)
        {
            agentIndex = Navigation.Crowd.AddAgent(DtCrowd, ref position, ref agentParams);
            return agentIndex >= 0;
        }

        public int AddAgent(float3 position, DtAgentParams agentParams)
        {
            return Navigation.Crowd.AddAgent(DtCrowd, ref position, ref agentParams);
        }

        public void RemoveAgent(int idx)
        {
            Navigation.Crowd.RemoveAgent(DtCrowd, idx);
        }

        public bool RequestMoveAgent(int idx, float3 position)
        {
            return Navigation.Crowd.RequestMoveAgent(DtCrowd, idx, ref position) == 1;
        }

        public void SetAgentParams(int idx, DtAgentParams agentParams)
        {
            Navigation.Crowd.SetAgentParams(DtCrowd, idx, ref agentParams);
        }

        public unsafe DtAgentParams GetAgentParams(int idx)
        {
            DtAgentParams result = default; ;
          
            Navigation.Crowd.GetAgentParams(DtCrowd, idx, new IntPtr(&result));
            return result;
        }

        public DtCrowdAgent GetAgent(int idx)
        {
            DtCrowdAgent result = default;

            Navigation.Crowd.GetAgent(DtCrowd, idx, new IntPtr(&result));
            return result;
        }

        public int GetAgentCount()
        {
            return Navigation.Crowd.GetAgentCount(DtCrowd);
        }

        public void Update(float dt)
        {
            Navigation.Crowd.Update(DtCrowd, dt);
        }

        public int GetAgents(List<DtCrowdAgent> agents, int max)
        {
            DtCrowdAgentsResult result = default;
            DtCrowdAgent[] generatedAgents = new DtCrowdAgent[max];
            fixed (DtCrowdAgent* agentsPtr = generatedAgents)
            {
                result.Agents = new IntPtr(agentsPtr);
                Navigation.Crowd.GetAgents(DtCrowd, new IntPtr(&result));
            }

            DtCrowdAgent* resultAgents = (DtCrowdAgent*)result.Agents;
            for (int i = 0; i < result.AgentCount; i++)
            {
                agents.Add(resultAgents[i]);
            }
            return result.AgentCount;
        }

        public int GetAgents(NativeArray<DtCrowdAgent> agents, int max)
        {
            if (agents.Length < max)
            {
                throw new ArgumentException("Array length < specified max");
            }
            return GetAgents((DtCrowdAgent*)agents.GetUnsafePtr(), max);
        }

        public int GetAgents(DtCrowdAgent* agents, int max)
        {
            DtCrowdAgentsResult result;
            result.Agents = new IntPtr(agents);
            Navigation.Crowd.GetAgents(DtCrowd, new IntPtr(&result));

            return result.AgentCount;
        }
        
    }
}
