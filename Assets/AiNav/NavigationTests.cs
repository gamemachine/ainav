using AiNav;
using AiNav.Collections;
using GameCommon;
using NUnit.Framework;
using System.Collections.Generic;
using System.Linq;
using Unity.Mathematics;

namespace Game.Test
{
    internal class NavigationTests
    {

        [Test]
        public void QueryInvalidates()
        {
            AiNavMesh navmesh = LoadMesh();
            AiNavQuery query = new AiNavQuery(navmesh, 1024);
            Assert.IsTrue(query.IsValid());
            query.Invalidate();
            Assert.IsFalse(query.IsValid());
            query.Dispose();
            navmesh.Dispose();
        }

        [Test]
        public void DisposeNavmeshInvalidatesQuery()
        {
            AiNavMesh navmesh = LoadMesh();
            AiNavQuery query = new AiNavQuery(navmesh, 1024);
            Assert.IsTrue(query.IsValid());
            navmesh.Dispose();
            Assert.IsFalse(query.IsValid());
            query.Dispose();
        }

        [Test]
        public void BuildTiles()
        {
            
            NavMeshBuildSettings buildSettings = NavMeshBuildSettings.Default();
            NavAgentSettings agentSettings = NavAgentSettings.Default();
            NavMeshBuilder builder = new NavMeshBuilder(buildSettings, agentSettings);

            NavMeshTestData data = NavMeshTestData.Load();
           
            data.GetInputData(out float3[] vertices, out int[] indices);
            GameLogger.Log("BuildTiles Inputs Vertices:{0} Triangles{1}", vertices.Length, indices.Length);

            NavMeshInputBuilder input = new NavMeshInputBuilder(default);
            input.Append(vertices, indices, DtArea.WALKABLE);
            
            builder.BuildAllFromSingleInput(input.ToBuildInput());
            GameLogger.Log("BuildResult: {0}", builder.BuildResult);

            Assert.AreEqual(0, builder.BuildResult.Result);

            data.Tiles = builder.Tiles.Values.Select(t => t.Data).ToList();
            data.Save();
            input.Dispose();

            Dictionary<byte, int> stats = new Dictionary<byte, int>();
            foreach (NavMeshTile tile in builder.Tiles.Values)
            {
                tile.AppendStats(stats);
            }
            foreach(KeyValuePair<byte,int> pair in stats)
            {
                GameLogger.Log("AreaAndType:{0} count:{1}", pair.Key, pair.Value);
            }
            

            AiNativeList<float3> outVerts = new AiNativeList<float3>(2);
            AiNativeList<int> outIndices = new AiNativeList<int>(2);
            data.GetTileGeometry(outVerts, outIndices);
            GameLogger.Log("BuildTiles Output Vertices:{0} Triangles{1}", outVerts.Length, outIndices.Length);
            outVerts.Dispose();
            outIndices.Dispose();

        }

        [Test]
        public unsafe void HasPath()
        {
            AiNavMesh navmesh = LoadMesh();

            AiNavQuery query = new AiNavQuery(navmesh, 1024);

            NavQuerySettings querySettings = NavQuerySettings.Default;
            float3 start = new float3(1f, 0f, 1f);
            float3 end = new float3(250f, 0f, 250f);
            bool hasPath = query.HasPath(querySettings, start, end);
            navmesh.Dispose();
            query.Dispose();
            Assert.IsTrue(hasPath);

        }

        [Test]
        public unsafe void FindPath()
        {
            AiNavMesh navmesh = LoadMesh();

            AiNavQuery query = new AiNavQuery(navmesh, 1024);

            NavQuerySettings querySettings = NavQuerySettings.Default;
            AiNativeArray<float3> path = new AiNativeArray<float3>(querySettings.MaxPathPoints);
            float3 start = new float3(1f, 0f, 1f);
            float3 end = new float3(250f, 0f, 250f);
            bool found = query.TryFindPath(querySettings, start, end, (float3*)path.GetUnsafePtr(), out int pathLength);
            
            Assert.IsTrue(found);
            GameLogger.Log("{0} {1}", pathLength, path[pathLength - 1]);

            navmesh.Dispose();
            path.Dispose();
            query.Dispose();
        }

        [Test]
        public void CreateCrowd()
        {
            AiNavMesh navmesh = LoadMesh();

            AiCrowd crowd = new AiCrowd(navmesh.DtNavMesh);

            DtAgentParams agentParams = DtAgentParams.Default;
            crowd.Dispose();
            navmesh.Dispose();
        }

        [Test]
        public void SetGetAgentParams()
        {
            AiNavMesh navmesh = LoadMesh();
            AiCrowd crowd = new AiCrowd(navmesh.DtNavMesh);

            DtAgentParams agentParams = DtAgentParams.Default;
            float3 position = new float3(2f, 0f, 2f);
            int idx = crowd.AddAgent(position, agentParams);

            DtAgentParams current = crowd.GetAgentParams(idx);
            Assert.IsTrue(agentParams.Equals(current));

            crowd.SetAgentParams(idx, agentParams);
            current = crowd.GetAgentParams(idx);
            Assert.IsTrue(agentParams.Equals(current));

            agentParams.Height = 5f;
            agentParams.AnticipateTurns = 0;
            agentParams.OptimizeVis = 0;
            crowd.SetAgentParams(idx, agentParams);
            current = crowd.GetAgentParams(idx);
            Assert.AreEqual(5f, current.Height);
            Assert.AreEqual(0, current.AnticipateTurns);
            Assert.AreEqual(0, current.OptimizeVis);

            crowd.Dispose();
            navmesh.Dispose();
        }

        [Test]
        public void CrowdPerfTest()
        {
            int agentCount = 100;
            float time = 1f;
            AiNavMesh navmesh = LoadMesh();
            AiNavQuery query = new AiNavQuery(navmesh, 1024);

            AiCrowd crowd = new AiCrowd(navmesh.DtNavMesh, agentCount);
            crowd.Update(time);

            DtAgentParams agentParams = DtAgentParams.Default;
            for (int i = 0; i < agentCount; i++)
            {
                float3 target = default;
                query.GetRandomPosition(ref target);
                int idx = crowd.AddAgent(target, agentParams);

                query.GetRandomPosition(ref target);
                bool moved = crowd.RequestMoveAgent(idx, target);
                Assert.IsTrue(moved);
            }

            time += 0.33f;
            crowd.Update(time);

            var watch = System.Diagnostics.Stopwatch.StartNew();

            for (int i = 0; i < 10; i++)
            {
                time += 0.33f;
                crowd.Update(time);
            }
            watch.Stop();
            GameLogger.Log("elapsed {0}", watch.ElapsedMilliseconds);

            crowd.Dispose();
            query.Dispose();
            navmesh.Dispose();
        }

        [Test]
        public void MoveAgent()
        {
            AiNavMesh navmesh = LoadMesh();
            AiNavQuery query = new AiNavQuery(navmesh, 1024);
            AiCrowd crowd = new AiCrowd(navmesh.DtNavMesh);

            DtAgentParams agentParams = DtAgentParams.Default;
            float3 position = new float3(2f, 0f, 2f);
            int idx = crowd.AddAgent(position, agentParams);

            float3 target = default;
            if (!query.GetRandomPosition(ref target))
            {
                Assert.Fail();
            }
            crowd.RequestMoveAgent(idx, target);
            crowd.Update(20f);
            DtCrowdAgent agent = crowd.GetAgent(idx);
            Assert.AreEqual(1, agent.Active);
            GameLogger.Log("position:{0} speed:{1} velocity:{2}", agent.Position, agent.DesiredSpeed, agent.Velocity);
            float distance = math.distance(position, agent.Position);
            Assert.IsTrue(distance > 1f);

            query.Dispose();
            crowd.Dispose();
            navmesh.Dispose();
        }

        [Test]
        public unsafe void AddRemoveQueryAgents()
        {
            AiNavMesh navmesh = LoadMesh();
            AiCrowd crowd = new AiCrowd(navmesh.DtNavMesh);

            DtAgentParams agentParams = DtAgentParams.Default;
            float3 position = new float3(2f, 0f, 2f);
            int idx = crowd.AddAgent(position, agentParams);
            Assert.AreEqual(0, idx);

            idx = crowd.AddAgent(position, agentParams);
            Assert.AreEqual(1, idx);

            AiNativeArray<DtCrowdAgent> agents = new AiNativeArray<DtCrowdAgent>(100);
            int agentCount = crowd.GetAgents((DtCrowdAgent*)agents.GetUnsafePtr(), 100);
            Assert.AreEqual(2, agentCount);

            for(int i=0;i<agentCount;i++)
            {
                DtCrowdAgent agent = agents[i];
                GameLogger.Log("Agent position:{0} speed:{1} velocity:{2} partial:{3}", agent.Position, agent.DesiredSpeed, agent.Velocity, agent.Partial);
            }
            agents.Dispose();

            agentCount = crowd.GetAgentCount();
            Assert.AreEqual(2, agentCount);

            crowd.RemoveAgent(0);
            crowd.RemoveAgent(1);

            agentCount = crowd.GetAgentCount();
            Assert.AreEqual(0, agentCount);

            crowd.Dispose();
            navmesh.Dispose();
        }

        [Test]
        public void SamplePositionTest()
        {
            AiNavMesh navmesh = LoadMesh();
            AiNavQuery query = new AiNavQuery(navmesh, 1024);

            float3 onMesh = default;
            float3 point = new float3(2f, 0f, 2f);
            float3 extent = new float3(4f, 4f, 4f);
            bool result = query.SamplePosition(point, extent, out onMesh);
            Assert.IsTrue(result);

            point = new float3(3000f, 1000f, 3000f);
            extent = new float3(4000f, 4000f, 4000f);
            result = query.SamplePosition(point, extent, out onMesh);
            Assert.IsTrue(result);

            GameLogger.Log("sampled {0}", onMesh);

            query.Dispose();
            navmesh.Dispose();
        }

        [Test]
        public void GetLocation()
        {
            AiNavMesh navmesh = LoadMesh();
            AiNavQuery query = new AiNavQuery(navmesh, 1024);

            float3 point = new float3(2f, 0f, 2f);
            float3 extent = new float3(4f, 4f, 4f);
            bool result = query.GetLocation(point, extent, out float3 onMesh);
            Assert.IsTrue(result);
            GameLogger.Log("GetLocation {0}", onMesh);
            query.Dispose();
            navmesh.Dispose();
        }

        [Test]
        public void GetRandomPosition()
        {
            AiNavMesh navmesh = LoadMesh();
            AiNavQuery query = new AiNavQuery(navmesh, 1024);

            float3 onMesh = default;
            bool result = query.GetRandomPosition(ref onMesh);
            Assert.IsTrue(result);
            GameLogger.Log("random {0}", onMesh);
            query.Dispose();
            navmesh.Dispose();
        }

        private AiNavMesh LoadMesh()
        {
            NavMeshTestData data = NavMeshTestData.Load();
            Assert.IsTrue(data.Tiles.Count > 0);
            NavMeshBuildSettings buildSettings = NavMeshBuildSettings.Default();
            AiNavMesh navmesh = new AiNavMesh(buildSettings.TileSize, buildSettings.CellSize);
            navmesh.AddOrReplaceTiles(data.Tiles);
            return navmesh;
        }
    }
}
