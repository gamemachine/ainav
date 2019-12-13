using System;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Mathematics;

namespace AiNav
{
    // A query belongs to a navmesh and once the navmesh is disposed, it can no longer be used.  It will simply return invalid results once it's navmesh is disposed.
    //  Queries should only be allocated on the main thread, and should only be used in one thread at a time.  No parallel Unity jobs for example should use the same query.
    public struct AiNavQuery
    {
        private static int NextId = 1;

        public int NavMeshId;
        public int Id;
        [NativeDisableUnsafePtrRestriction]
        public IntPtr DtQuery;

        public AiNavQuery(AiNavMesh navmesh, int maxNodes)
        {
            NavMeshId = navmesh.Id;
            
            Id = NextId;
            NextId++;

            DtQuery = Navigation.Query.Create(navmesh.DtNavMesh, maxNodes);
            if (DtQuery == IntPtr.Zero)
            {
                throw new ApplicationException("Unable to create query");
            }

            AiNavWorld.Instance.RegisterQuery(this);
        }

        public void Dispose()
        {
            AiNavWorld.Instance.UnregisterQuery(Id);
            if (DtQuery != IntPtr.Zero)
            {
                Navigation.Query.Destroy(DtQuery);
                DtQuery = IntPtr.Zero;
            }
        }

        public unsafe bool HasPath(NavQuerySettings querySettings, float3 start, float3 end)
        {
            if (DtQuery == IntPtr.Zero)
                return false;

            DtPathFindQuery query;
            query.Source = start;
            query.Target = end;
            query.MaxPathPoints = querySettings.MaxPathPoints;
            query.FindNearestPolyExtent = querySettings.FindNearestPolyExtent;

            int result = Navigation.Query.HasPath(DtQuery, ref query);
            return result == 1;
        }

        public unsafe bool TryFindPath(NavQuerySettings querySettings, float3 start, float3 end, float3* path, out int pathLength)
        {
            pathLength = 0;
            if (DtQuery == IntPtr.Zero)
                return false;

            DtPathFindQuery query;
            query.Source = start;
            query.Target = end;
            query.MaxPathPoints = querySettings.MaxPathPoints;
            query.FindNearestPolyExtent = querySettings.FindNearestPolyExtent;
            DtPathFindResult queryResult;

            queryResult.PathPoints = new IntPtr(path);
            Navigation.Query.FindStraightPath(DtQuery, ref query, new IntPtr(&queryResult));
            if (!queryResult.PathFound)
                return false;

            pathLength = queryResult.NumPathPoints;
            return true;
        }

        // SamplePosition does not use the detail mesh, height will not match surface
        public bool SamplePosition(float3 point, float range, out float3 result)
        {
            float3 extent = new float3(range, range, range);
            return Navigation.Query.SamplePosition(DtQuery, ref point, ref extent, out result) == 1;
        }

        public bool SamplePosition(float3 point, float3 extent, out float3 result)
        {
            return Navigation.Query.SamplePosition(DtQuery, ref point, ref extent, out result) == 1;
        }

        // GetLocation is the same as SamplePosition but it does use the detail mesh, returning the surface height
        public bool GetLocation(float3 point, float3 extent, out float3 result)
        {
            return Navigation.Query.GetLocation(DtQuery, ref point, ref extent, out result) == 1;
        }

        public bool GetRandomPosition(ref float3 result)
        {
            return Navigation.Query.GetRandomPosition(DtQuery, ref result) == 1;
        }

        public bool IsValid()
        {
            return Navigation.Query.IsValid(DtQuery) == 1;
        }

        public void Invalidate()
        {
            Navigation.Query.Invalidate(DtQuery);
        }

    }
}
