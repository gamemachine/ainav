using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;

namespace AiNav
{
    public partial class CrowdController
    {
        private bool GenerateAgentDebugData = true;
        private const float CrowdTicksPerSecond = 30f;
        private const int MaxAgents = 1024;

        public AiCrowd AiCrowd { get; private set; }
        private SurfaceController AiNavSurfaceController;
        private AiNavMesh NavMesh;
        private AiNavQuery Query;
        private GameClock UpdateClock;

        private NativeArray<int> AgentCount;
        private NativeHashMap<int, NavAgentDebug> ReadOnlyAgents;
        private NativeArray<float3> Path;

        public CrowdController(AiNavMesh navMesh, SurfaceController surfaceController)
        {
            NavMesh = navMesh;
            AiNavSurfaceController = surfaceController;

            AiCrowd = new AiCrowd(NavMesh.DtNavMesh);
            Query = new AiNavQuery(NavMesh, 2048);
            AiCrowd.Update(Time.time);

            UpdateClock = new GameClock(CrowdTicksPerSecond);

            AgentCount = new NativeArray<int>(1, Allocator.Persistent);
            ReadOnlyAgents = new NativeHashMap<int, NavAgentDebug>(MaxAgents, Allocator.Persistent);
            Path = new NativeArray<float3>(1024, Allocator.Persistent);
        }

        public void OnDestroy()
        {
            if (AgentCount.IsCreated) AgentCount.Dispose();
            if (ReadOnlyAgents.IsCreated) ReadOnlyAgents.Dispose();
            if (Path.IsCreated) Path.Dispose();
            Query.Dispose();
            AiCrowd.Dispose();

        }

        public int GetAgentCount()
        {
            return AgentCount[0];
        }

        public bool TryGetAgent(int crowdIndex, out NavAgentDebug debug)
        {
            return ReadOnlyAgents.TryGetValue(crowdIndex, out debug);
        }

        public bool TryAddAgent(float3 position, DtAgentParams agentParams, out NavAgent agent)
        {
            
            agent = new NavAgent
            {
                Active = true,
                DtAgentParams = agentParams,
                Destination = position
            };

            if (AiCrowd.TryAddAgent(position, agentParams, out int crowdIndex))
            {
                agent.CrowdIndex = crowdIndex;
                return true;
            }
            else
            {
                return false;
            }
        }

        public void RemoveAgent(int crowdIndex)
        {
            if (crowdIndex >= 0)
            {
                AiCrowd.RemoveAgent(crowdIndex);
            }
        }

        public JobHandle OnUpdate(AiNavSystem system, JobHandle inputDeps)
        {
            if (AiNavSurfaceController.Building)
            {
                return inputDeps;
            }

            if (UpdateClock.Tick(Time.time))
            {
                CrowdAgentsJob updateAgentJob = new CrowdAgentsJob
                {
                    Debug = GenerateAgentDebugData,
                    AiCrowd = AiCrowd,
                    Query = Query,
                    ReadOnlyAgents = ReadOnlyAgents,
                    PathLookup = system.GetBufferFromEntity<AgentPathBuffer>(false),
                    NavQuerySettings = NavQuerySettings.Default,
                    Path = Path
                };

                inputDeps = updateAgentJob.ScheduleSingle(system, inputDeps);

                TickCrowdJob updateJob = new TickCrowdJob
                {
                    DeltaTime = Time.deltaTime,
                    AiCrowd = AiCrowd,
                    AgentCount = AgentCount
                };
                inputDeps = updateJob.Schedule(inputDeps);
            }

            return inputDeps;
        }
        
    }
}
