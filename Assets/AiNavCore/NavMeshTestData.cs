using AiNav.Collections;
using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using Unity.Mathematics;
using UnityEngine;

namespace AiNav
{
    [Serializable]
    public class NavMeshTestData
    {
        public byte[] InputVerts;
        public byte[] InputIndices;
        public List<byte[]> Tiles = new List<byte[]>();

        private const string Directory = @"C:\Games\aigame\navigation_data";

        public void SetTiles(Dictionary<int2, NavMeshTile> tiles)
        {
            Tiles.Clear();
            foreach(NavMeshTile tile in tiles.Values)
            {
                Tiles.Add(tile.Data);
            }
        }

        public static NavMeshTestData Load()
        {
            IFormatter formatter = new BinaryFormatter();
            Stream stream = new FileStream(GetPath(), FileMode.Open, FileAccess.Read, FileShare.Read);
            NavMeshTestData data = (NavMeshTestData)formatter.Deserialize(stream);
            stream.Close();

            return data;
        }

        private static string GetPath()
        {
            EnsureDirectory();
            return Path.Combine(Directory, string.Format("{0}.pb", "NavMeshTestData"));
        }

        private static void EnsureDirectory()
        {
            if (string.IsNullOrEmpty(Directory))
            {
                throw new Exception("Path is null");
            }

            if (!System.IO.Directory.Exists(Directory))
            {
                System.IO.Directory.CreateDirectory(Directory);
            }
        }

        public void Save()
        {
            IFormatter formatter = new BinaryFormatter();
            Stream stream = new FileStream(GetPath(), FileMode.Create, FileAccess.Write, FileShare.None);
            formatter.Serialize(stream, this);
            stream.Close();
        }

        public Mesh ToMesh()
        {
            AiNativeList<float3> vertices = new AiNativeList<float3>(2);
            AiNativeList<int> indices = new AiNativeList<int>(2);
           
            GetTileGeometry(vertices, indices);
            Vector3[] unityVerts = new Vector3[vertices.Length];
            for (int i = 0; i < vertices.Length; i++)
            {
                unityVerts[i] = new Vector3(vertices[i].x, vertices[i].y, vertices[i].z);
            }

            Mesh mesh = new Mesh();
            mesh.indexFormat = UnityEngine.Rendering.IndexFormat.UInt32;
            mesh.vertices = unityVerts;
            mesh.uv = new Vector2[unityVerts.Length];
            mesh.triangles = indices.ToArray();
            mesh.RecalculateBounds();
            mesh.RecalculateNormals();

            vertices.Dispose();
            indices.Dispose();
            return mesh;
        }

        public void SetInputs(Mesh mesh)
        {
            InputVerts = mesh.vertices.ToByteArray();
            InputIndices = mesh.triangles.ToByteArray();
            Save();
        }

        public void GetInputData(out float3[] vertices, out int[] indices)
        {
            vertices = InputVerts.ToArray<float3>();
            indices = InputIndices.ToArray<int>();
        }

        public void GetTileGeometry(AiNativeList<float3> vertices, AiNativeList<int> indices)
        {
            AiNativeList<float3> tileVerts = new AiNativeList<float3>(2);
            AiNativeList<int> tileIndices = new AiNativeList<int>(2);
            foreach (byte[] data in Tiles)
            {
                NavMeshTile tile = new NavMeshTile();
                tile.Data = data;
                tile.GetTileVertices(tileVerts, tileIndices);

                // Copy vertices
                int vbase = vertices.Length;
                for (int i = 0; i < tileVerts.Length; i++)
                {
                    vertices.Add(tileVerts[i]);
                }

                // Copy indices with offset applied
                for (int i = 0; i < tileIndices.Length; i++)
                {
                    indices.Add(tileIndices[i] + vbase);
                }

                tileVerts.Clear();
                tileIndices.Clear();
            }

            tileVerts.Dispose();
            tileIndices.Dispose();
        }
        

    }
}
