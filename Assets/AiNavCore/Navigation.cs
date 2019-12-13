using System;
using System.Runtime.InteropServices;
using System.Security;
using Unity.Mathematics;

namespace AiNav
{
    [SuppressUnmanagedCodeSecurity]
    public class Navigation
    {

        //private const string NativeLibrary = @"C:\Games\aigame\AiNav\x64\Release\AiNav.dll";
        private const string NativeLibrary = "AiNav";
        //private const string NativeLibrary = @"C:\Games\aigame\AiNav\x64\Debug\AiNav.dll";

        //[SuppressUnmanagedCodeSecurity]
        [DllImport(NativeLibrary, CallingConvention = CallingConvention.Cdecl)]
        public static extern int GetVersion();

        

        public class NavMesh
        {
            // Navmesh generation API
            [SuppressUnmanagedCodeSecurity]
            [DllImport(NativeLibrary, EntryPoint = "CreateBuilder", CallingConvention = CallingConvention.Cdecl)]
            public static extern IntPtr CreateBuilder();

            [SuppressUnmanagedCodeSecurity]
            [DllImport(NativeLibrary, EntryPoint = "DestroyBuilder", CallingConvention = CallingConvention.Cdecl)]
            public static extern void DestroyBuilder(IntPtr builder);

            [SuppressUnmanagedCodeSecurity]
            [DllImport(NativeLibrary, EntryPoint = "BuildNavmesh", CallingConvention = CallingConvention.Cdecl)]
            public static extern IntPtr Build(IntPtr builder,
                [MarshalAs(UnmanagedType.LPArray, SizeParamIndex = 2)] float3[] verts, int numVerts,
                [MarshalAs(UnmanagedType.LPArray, SizeParamIndex = 4)] int[] inds, int numInds);

            [SuppressUnmanagedCodeSecurity]
            [DllImport(NativeLibrary, EntryPoint = "BuildNavmesh", CallingConvention = CallingConvention.Cdecl)]
            public static unsafe extern IntPtr Build2(IntPtr builder, float3* verts, int numVerts, int* inds, int numInds, byte* areas);

            [SuppressUnmanagedCodeSecurity]
            [DllImport(NativeLibrary, EntryPoint = "SetSettings", CallingConvention = CallingConvention.Cdecl)]
            public static extern void SetSettings(IntPtr builder, IntPtr settings);

            /// <summary>
            /// Creates a new navigation mesh object. 
            /// You must add tiles to it with AddTile before you can perform navigation queries using Query
            /// </summary>
            /// <returns></returns>
            [SuppressUnmanagedCodeSecurity]
            [DllImport(NativeLibrary, EntryPoint = "CreateNavmesh", CallingConvention = CallingConvention.Cdecl)]
            public static extern IntPtr CreateNavmesh(float cellTileSize);

            [SuppressUnmanagedCodeSecurity]
            [DllImport(NativeLibrary, EntryPoint = "DestroyNavmesh", CallingConvention = CallingConvention.Cdecl)]
            public static extern IntPtr DestroyNavmesh(IntPtr query);

            /// <summary>
            /// Adds a new tile to the navigation mesh object
            /// </summary>
            /// <param name="navmesh"></param>
            /// <param name="data">Navigation mesh binary data in the detour format to load</param>
            /// <param name="dataLength">Length of the binary mesh data</param>
            [SuppressUnmanagedCodeSecurity]
            [DllImport(NativeLibrary, EntryPoint = "AddTile", CallingConvention = CallingConvention.Cdecl)]
            public static extern int AddTile(IntPtr navmesh, IntPtr data, int dataLength);

            /// <summary>
            /// Removes a tile from the navigation mesh object
            /// </summary>
            /// <param name="navmesh"></param>
            /// <param name="tileCoordinate">Coordinate of the tile to remove</param>
            [SuppressUnmanagedCodeSecurity]
            [DllImport(NativeLibrary, EntryPoint = "RemoveTile", CallingConvention = CallingConvention.Cdecl)]
            public static extern int RemoveTile(IntPtr navmesh, int2 tileCoordinate);
        }


        public class Query
        {
            [SuppressUnmanagedCodeSecurity]
            [DllImport(NativeLibrary, EntryPoint = "QueryCreate", CallingConvention = CallingConvention.Cdecl)]
            public static extern IntPtr Create(IntPtr navmesh, int maxNodes);

            [SuppressUnmanagedCodeSecurity]
            [DllImport(NativeLibrary, EntryPoint = "QueryDestroy", CallingConvention = CallingConvention.Cdecl)]
            public static extern void Destroy(IntPtr aiQuery);

            [SuppressUnmanagedCodeSecurity]
            [DllImport(NativeLibrary, EntryPoint = "QueryIsValid", CallingConvention = CallingConvention.Cdecl)]
            public static extern int IsValid(IntPtr aiQuery);

            [SuppressUnmanagedCodeSecurity]
            [DllImport(NativeLibrary, EntryPoint = "QueryInvalidate", CallingConvention = CallingConvention.Cdecl)]
            public static extern int Invalidate(IntPtr aiQuery);


            [SuppressUnmanagedCodeSecurity]
            [DllImport(NativeLibrary, EntryPoint = "QueryFindStraightPath", CallingConvention = CallingConvention.Cdecl)]
            public static extern void FindStraightPath(IntPtr aiQuery, ref DtPathFindQuery pathFindQuery, IntPtr resultStructure);

            [SuppressUnmanagedCodeSecurity]
            [DllImport(NativeLibrary, EntryPoint = "QueryHasPath", CallingConvention = CallingConvention.Cdecl)]
            public static extern int HasPath(IntPtr aiQuery, ref DtPathFindQuery pathFindQuery);

            [SuppressUnmanagedCodeSecurity]
            [DllImport(NativeLibrary, EntryPoint = "QueryRaycast", CallingConvention = CallingConvention.Cdecl)]
            public static extern void Raycast(IntPtr aiQuery, DtRaycastQuery pathFindQuery, IntPtr resultStructure);


            [SuppressUnmanagedCodeSecurity]
            [DllImport(NativeLibrary, EntryPoint = "QuerySamplePosition", CallingConvention = CallingConvention.Cdecl)]
            public static extern int SamplePosition(IntPtr aiQuery, ref float3 point, ref float3 extent, out float3 result);

            [SuppressUnmanagedCodeSecurity]
            [DllImport(NativeLibrary, EntryPoint = "QueryGetRandomPosition", CallingConvention = CallingConvention.Cdecl)]
            public static extern int GetRandomPosition(IntPtr aiQuery, ref float3 result);

            [SuppressUnmanagedCodeSecurity]
            [DllImport(NativeLibrary, EntryPoint = "QueryGetLocation", CallingConvention = CallingConvention.Cdecl)]
            public static extern int GetLocation(IntPtr aiQuery, ref float3 point, ref float3 extent, out float3 result);
        }

        public class Crowd
        {
            [SuppressUnmanagedCodeSecurity]
            [DllImport(NativeLibrary, EntryPoint = "CrowdCreate", CallingConvention = CallingConvention.Cdecl)]
            public static extern IntPtr Create(IntPtr navmesh, int maxAgents, float maxRadius);

            [SuppressUnmanagedCodeSecurity]
            [DllImport(NativeLibrary, EntryPoint = "CrowdDestroy", CallingConvention = CallingConvention.Cdecl)]
            public static extern void Destroy(IntPtr crowd);

            [SuppressUnmanagedCodeSecurity]
            [DllImport(NativeLibrary, EntryPoint = "CrowdAddAgent", CallingConvention = CallingConvention.Cdecl)]
            public static extern int AddAgent(IntPtr crowd, ref float3 position, ref DtAgentParams agentParams);

            [SuppressUnmanagedCodeSecurity]
            [DllImport(NativeLibrary, EntryPoint = "CrowdRemoveAgent", CallingConvention = CallingConvention.Cdecl)]
            public static extern void RemoveAgent(IntPtr crowd, int idx);

            [SuppressUnmanagedCodeSecurity]
            [DllImport(NativeLibrary, EntryPoint = "CrowdSetAgentParams", CallingConvention = CallingConvention.Cdecl)]
            public static extern void SetAgentParams(IntPtr crowd, int idx, ref DtAgentParams agentParams);

            [SuppressUnmanagedCodeSecurity]
            [DllImport(NativeLibrary, EntryPoint = "CrowdGetAgentParams", CallingConvention = CallingConvention.Cdecl)]
            public static extern void GetAgentParams(IntPtr crowd, int idx, IntPtr agentParams);

            [SuppressUnmanagedCodeSecurity]
            [DllImport(NativeLibrary, EntryPoint = "CrowdGetAgentCount", CallingConvention = CallingConvention.Cdecl)]
            public static extern int GetAgentCount(IntPtr crowd);

            [SuppressUnmanagedCodeSecurity]
            [DllImport(NativeLibrary, EntryPoint = "CrowdRequestMoveAgent", CallingConvention = CallingConvention.Cdecl)]
            public static extern int RequestMoveAgent(IntPtr crowd, int idx, ref float3 position);

            [SuppressUnmanagedCodeSecurity]
            [DllImport(NativeLibrary, EntryPoint = "CrowdUpdate", CallingConvention = CallingConvention.Cdecl)]
            public static extern void Update(IntPtr crowd, float dt);

            [SuppressUnmanagedCodeSecurity]
            [DllImport(NativeLibrary, EntryPoint = "CrowdGetAgents", CallingConvention = CallingConvention.Cdecl)]
            public static extern void GetAgents(IntPtr crowd, IntPtr agents);

            [SuppressUnmanagedCodeSecurity]
            [DllImport(NativeLibrary, EntryPoint = "CrowdGetAgent", CallingConvention = CallingConvention.Cdecl)]
            public static extern int GetAgent(IntPtr crowd, int idx, IntPtr crowdAgent);
        }


        public static int DtAlign4(int size)
        {
            return (size + 3) & ~3;
        }
    }
}
