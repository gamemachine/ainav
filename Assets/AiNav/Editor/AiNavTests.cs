using NUnit.Framework;
using Unity.Mathematics;
using UnityEngine;

namespace AiNav
{
    [TestFixture]
    internal class AiNavTests
    {
       
        [Test]
        public unsafe void TestSourceMap()
        {
            GameObject gameObject = GameObject.CreatePrimitive(PrimitiveType.Cube);
            Mesh mesh = gameObject.GetComponent<MeshFilter>().sharedMesh;
            Object.DestroyImmediate(gameObject);

            var sharedData = MeshSourceData.Create(mesh);

            MeshSourceMap map = new MeshSourceMap(NavMeshBuildSettings.Default());
            float4x4 localToWorld = float4x4.TRS(default, default, new float3(40f, 40f, 40f));
            MeshSource source = MeshSource.Create(localToWorld, mesh, 0, 1, 200);
            source.Id = 1;
            map.Add(source);

            localToWorld = float4x4.TRS(new float3(100f, 100f, 100f), default, default);
            MeshSource sharedSource = MeshSource.CreateShared(localToWorld, sharedData, 0, 1, 100, 300);
            sharedSource.Id = 2;
            map.Add(sharedSource);

            int sourcesCount;
            int tileSourcesCount;

            map.GetCounts(out sourcesCount, out tileSourcesCount);
            Assert.AreEqual(2, sourcesCount);
            Assert.GreaterOrEqual(tileSourcesCount, 2);

            MeshSource foundSource = default;
            Assert.IsTrue(map.TryGetByCustomData(200, out foundSource));
            Assert.AreEqual(1, foundSource.Id);
            Assert.AreEqual(200, foundSource.Info.CustomData);

            Assert.IsTrue(map.Remove(1));

            Assert.IsFalse(map.TryGetByCustomData(200, out foundSource));

            map.GetCounts(out sourcesCount, out tileSourcesCount);
            Assert.AreEqual(1, sourcesCount);
            Assert.AreEqual(1, tileSourcesCount);

            Assert.IsTrue(map.Remove(2));

            map.GetCounts(out sourcesCount, out tileSourcesCount);
            Assert.AreEqual(0, sourcesCount);
            Assert.AreEqual(0, tileSourcesCount);

            map.Dispose();

        }

    }
}
