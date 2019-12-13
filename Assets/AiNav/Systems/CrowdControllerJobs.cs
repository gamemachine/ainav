using Unity.Burst;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;

namespace AiNav
{
    public partial class CrowdController
    {
        [BurstCompile]
        struct TickCrowdJob : IJob
        {
            public float DeltaTime;
            public AiCrowd AiCrowd;
            [NativeDisableContainerSafetyRestriction]
            public NativeArray<int> AgentCount;

            public void Execute()
            {
                AgentCount[0] = AiCrowd.GetAgentCount();
                AiCrowd.Update(DeltaTime);
            }
        }


        [BurstCompile]
        unsafe struct CrowdAgentsJob : IJobForEachWithEntity<NavAgent, AgentPathData>
        {
            public bool Debug;
            public AiCrowd AiCrowd;
            public AiNavQuery Query;
            public NavQuerySettings NavQuerySettings;
            public NativeArray<float3> Path;
            [NativeDisableContainerSafetyRestriction]
            public NativeHashMap<int, NavAgentDebug> ReadOnlyAgents;
            public BufferFromEntity<AgentPathBuffer> PathLookup;

            public void Execute(Entity entity, int index, ref NavAgent agent, ref AgentPathData pathData)
            {
                agent.DtCrowdAgent = AiCrowd.GetAgent(agent.CrowdIndex);

                if (UpdateActive(ref agent))
                {
                    return;
                }

                if (!agent.Active)
                {
                    return;
                }

                if (Debug)
                {
                    ReadOnlyAgents.Remove(agent.CrowdIndex);
                    NavAgentDebug debug = new NavAgentDebug();
                    debug.NavAgent = agent;
                    debug.PathData = pathData;
                    ReadOnlyAgents.TryAdd(agent.CrowdIndex, debug);
                }
                


                DynamicBuffer<AgentPathBuffer> pathBuffer = PathLookup[entity];

                pathData.DistanceToDestination = math.distance(agent.Destination, agent.DtCrowdAgent.Position);
                pathData.UsingPath = pathData.DistanceToDestination > 20f;

                if (agent.SetDestination)
                {
                    agent.SetDestination = false;
                    if (pathData.UsingPath)
                    {
                        bool success = Query.TryFindPath(NavQuerySettings, agent.DtCrowdAgent.Position, agent.Destination, (float3*)Path.GetUnsafePtr(), out int pathLength);
                        if (success && pathLength > 0)
                        {
                            pathData.CurrentIndex = 0;
                            PathHandler.CopyToBuffer(Path, pathLength, pathBuffer);
                            pathData.PathLength = pathLength;
                            pathData.LastQueryResult = 1;
                            agent.SetDestinationFailed = false;
                        } else
                        {
                            agent.SetDestinationFailed = true;
                            pathData.LastQueryResult = 0;
                        }
                    } else
                    {
                        agent.SetDestinationFailed = false;
                    }
                }

                PathHandler pathHandler = PathHandler.CreateFrom(pathData, pathBuffer);

                pathData.HasPath = pathHandler.HasPath;
                if (pathData.UsingPath && !pathData.HasPath)
                {
                    return;
                }

                if (pathData.DistanceToDestination <= 0.1f)
                {
                    return;
                }

                float3 target = agent.Destination;
                if (pathData.UsingPath)
                {
                    pathHandler.UpdatePathIndex(agent.DtCrowdAgent.Position, 0.5f);
                    pathData.CurrentIndex = pathHandler.CurrentIndex;
                    target = pathHandler.CurrentWaypoint;
                    pathData.CurrentWaypoint = target;
                    pathData.DistanceToWaypoint = math.distance(target, agent.DtCrowdAgent.Position);

                }

                AiCrowd.RequestMoveAgent(agent.CrowdIndex, target);


                if (agent.UpdateParams == 1)
                {
                    agent.UpdateParams = 0;
                    AiCrowd.SetAgentParams(agent.CrowdIndex, agent.DtAgentParams);
                    agent.DtAgentParams = AiCrowd.GetAgentParams(agent.CrowdIndex);
                }

            }

            private bool UpdateActive(ref NavAgent agent)
            {
                if (agent.Active && agent.CrowdIndex == -1)
                {
                    if (AiCrowd.TryAddAgent(agent.Destination, agent.DtAgentParams, out agent.CrowdIndex))
                    {
                        if (agent.CrowdIndex == -1)
                        {
                            throw new System.Exception("TryAddAgent failed - crowd index not set");
                        }
                        return true;
                    }
                }
                else if (!agent.Active && agent.CrowdIndex >= 0)
                {
                    AiCrowd.RemoveAgent(agent.CrowdIndex);
                    agent.CrowdIndex = -1;
                    return true;
                }

                return false;
            }

        }


    }
}
