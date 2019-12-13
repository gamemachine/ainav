using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Entities;
using Unity.Mathematics;

namespace AiNav
{
    public struct PathHandler
    {
        public int PathLength;
        public int CurrentIndex;
        public DynamicBuffer<AgentPathBuffer> Path;

        public static unsafe void CopyToBuffer(NativeArray<float3> path, int pathLength, DynamicBuffer<AgentPathBuffer> buffer)
        {
            if (buffer.Length < pathLength)
            {
                buffer.ResizeUninitialized(pathLength);
            }
            
            UnsafeUtility.MemCpy(buffer.GetUnsafePtr(), path.GetUnsafePtr(), pathLength * UnsafeUtility.SizeOf<float3>());
        }

        public static PathHandler CreateFrom(AgentPathData data, DynamicBuffer<AgentPathBuffer> pathBuffer)
        {
            PathHandler agentPath = new PathHandler();

            agentPath.Path = pathBuffer;
            agentPath.PathLength = data.PathLength;
            agentPath.CurrentIndex = data.CurrentIndex;

            return agentPath;
        }

        public bool AtPathEnd
        {
            get
            {
                return CurrentIndex == PathLength - 1;
            }
        }

        public bool HasPath
        {
            get
            {
                return PathLength > 0;
            }
        }

        public float3 LastPathPoint
        {
            get
            {
                return Path[PathLength - 1];
            }
        }

        public bool TryGetNextWaypoint(out float3 position)
        {
            position = default;
            if (HasPath && CurrentIndex + 1 <= PathLength - 1)
            {
                position = Path[CurrentIndex + 1];
                return true;
            }
            else
            {
                return false;
            }
        }

        public float3 CurrentWaypoint
        {
            get
            {
                if (!HasPath)
                {
                    return default;
                }
                return Path[CurrentIndex];
            }
        }

        public void UpdatePathIndex(float3 currentPosition, float reachedDistance)
        {
            if (!HasPath)
            {
                return;
            }

            float3 currentWaypoint = CurrentWaypoint;
            float distance = math.distance(currentWaypoint, currentPosition);

            if (distance <= reachedDistance)
            {
                int nextIndex = CurrentIndex + 1;
                if (nextIndex <= PathLength - 1)
                {
                    CurrentIndex = nextIndex;
                }
            }

        }
    }
}
