using System.Collections.Generic;
using System.IO;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Entities;
using Unity.Entities.Serialization;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace AiNav
{
    public class NavMeshStoreSystem : ComponentSystem
    {
        private const bool SurfacesSceneScoped = true;

        public static NavMeshStoreSystem Instance => EcsWorld.Active.GetOrCreateSystem<NavMeshStoreSystem>();
        private string CurrentSceneName;
        
        public unsafe bool SaveTiles(Dictionary<int2, NavMeshTile> tiles, int surfaceId)
        {
            string path = GetPath(surfaceId, typeof(NavMeshTile).Name, null);

            using (StreamBinaryWriter writer = new StreamBinaryWriter(path))
            {
                writer.Write(tiles.Count);
                foreach (NavMeshTile tile in tiles.Values)
                {
                    writer.Write(tile.Data.Length);
                    fixed (void* dataPtr = tile.Data)
                    {
                        writer.WriteBytes(dataPtr, tile.Data.Length);
                    }
                }
            }
            return true;
        }

        public unsafe bool LoadTiles(int surfaceId, Dictionary<int2, NavMeshTile> tiles)
        {
            string path = GetPath(surfaceId, typeof(NavMeshTile).Name, null);
            if (!File.Exists(path))
            {
                //Debug.LogFormat("Tiles not found id:{0}", surfaceId);
                return false;
            }

            using (StreamBinaryReader reader = new StreamBinaryReader(path))
            {
                int tileCount = reader.ReadInt();
                for (int i = 0; i < tileCount; i++)
                {
                    int tileSize = reader.ReadInt();
                    byte[] data = new byte[tileSize];
                    fixed (byte* dataPtr = data)
                    {
                        reader.ReadBytes(dataPtr, tileSize);
                    }
                    NavMeshTile tile = new NavMeshTile();
                    tile.Data = data;
                    tiles[tile.Coord] = tile;
                }
            }
            return true;
        }

        public bool HasTiles(int surfaceId)
        {
            string path = GetPath(surfaceId, typeof(NavMeshTile).Name, null);
            return File.Exists(path);
        }

        public bool Exists<T>(int surfaceId, string featureId = null)
        {
            string path = GetPath(surfaceId, typeof(T).Name, featureId);
            return File.Exists(path);
        }

        public void DestroySurfaceData(int surfaceId)
        {
            string surfacePath = GetSurfacePath(surfaceId);
            if (Directory.Exists(surfacePath))
            {
                Directory.Delete(surfacePath, true);
            }
        }

        public unsafe NativeArray<BlobAssetReference<MeshSourceData>> LoadSources(int surfaceId)
        {
            string path;
            if (surfaceId == -1)
            {
                path = GetGlobalPath(typeof(MeshSourceData).Name);
            } else
            {
                path = GetPath(surfaceId, typeof(MeshSourceData).Name, null);
            }

            using (StreamBinaryReader reader = new StreamBinaryReader(path))
            {
                int sourceCount = reader.ReadInt();
                NativeArray<BlobAssetReference<MeshSourceData>> sources = new NativeArray<BlobAssetReference<MeshSourceData>>(sourceCount, Allocator.Temp);

                for (int i = 0; i < sourceCount; i++)
                {
                    int sizeInBytes = reader.ReadInt();
                    NativeArray<int> indices = new NativeArray<int>(sizeInBytes, Allocator.Temp);
                    reader.ReadBytes(indices.GetUnsafePtr(), sizeInBytes);

                    sizeInBytes = reader.ReadInt();
                    NativeArray<float3> vertices = new NativeArray<float3>(sizeInBytes, Allocator.Temp);
                    reader.ReadBytes(vertices.GetUnsafePtr(), sizeInBytes);

                    sources[i] = MeshSourceData.Create(indices, vertices);
                    indices.Dispose();
                    vertices.Dispose();
                }
                return sources;
            }
        }

        public unsafe void SaveSources(NativeArray<BlobAssetReference<MeshSourceData>> sources, int surfaceId)
        {
            string path;
            if (surfaceId == -1)
            {
                path = GetGlobalPath(typeof(MeshSourceData).Name);
            }
            else
            {
                path = GetPath(surfaceId, typeof(MeshSourceData).Name, null);
            }

            using (StreamBinaryWriter writer = new StreamBinaryWriter(path))
            {
                writer.Write(sources.Length);

                for (int i = 0; i < sources.Length; i++)
                {
                    ref var source = ref sources[i].Value;

                    int sizeInBytes = source.Indices.Length * UnsafeUtility.SizeOf<int>();
                    writer.Write(sizeInBytes);
                    writer.WriteBytes(source.Indices.GetUnsafePtr(), sizeInBytes);

                    sizeInBytes = source.Vertices.Length * UnsafeUtility.SizeOf<float3>();
                    writer.Write(sizeInBytes);
                    writer.WriteBytes(source.Vertices.GetUnsafePtr(), sizeInBytes);
                }
            }
        }

        public unsafe void SaveArray<T>(NativeArray<T> data, int surfaceId, string featureId = null) where T : struct
        {
            string path = GetPath(surfaceId, typeof(T).Name, featureId);

            using (StreamBinaryWriter writer = new StreamBinaryWriter(path))
            {
                writer.Write(data.Length);
                writer.WriteBytes(data.GetUnsafeReadOnlyPtr(), data.Length * UnsafeUtility.SizeOf<T>());
            }
        }


        public unsafe void LoadArray<T>(NativeArray<T> data, int surfaceId, string featureId = null) where T : struct
        {
            string path = GetPath(surfaceId, typeof(T).Name, featureId);

            using (StreamBinaryReader reader = new StreamBinaryReader(path))
            {
                int count = reader.ReadInt();
                reader.ReadBytes(data.GetUnsafePtr(), count * UnsafeUtility.SizeOf<T>());
            }
        }

        public unsafe int GetArrayLength<T>(int surfaceId, string featureId = null) where T : struct
        {
            string path = GetPath(surfaceId, typeof(T).Name, featureId);

            using (StreamBinaryReader reader = new StreamBinaryReader(path))
            {
                return reader.ReadInt();
            }
        }

        protected override void OnCreate()
        {
            base.OnCreate();

            if (SurfacesSceneScoped)
            {
                SceneManager.sceneLoaded += OnSceneLoaded;
                CurrentSceneName = SceneManager.GetActiveScene().name;
                //Debug.LogFormat("NavMeshStore Init {0}", CurrentSceneName);
            }
            else
            {
                CurrentSceneName = "Global";
            }
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();

        }

        protected override void OnUpdate()
        {
        }

        private string GetPath(int surfaceId, string name, string featureId)
        {
            string surfacePath = GetSurfacePath(surfaceId);
            string featureStr = string.IsNullOrEmpty(featureId) ? "default" : featureId;

            return Path.Combine(surfacePath, string.Format("{0}_{1}.bin", name, featureStr));
        }

        private string GetGlobalPath(string name)
        {
            string basePath = Path.Combine(Application.streamingAssetsPath, "AiNav");
            if (!Directory.Exists(basePath))
            {
                Directory.CreateDirectory(basePath);
            }
            return Path.Combine(basePath, string.Format("{0}.bin", name));
        }

        private string GetSurfacePath(int surfaceId)
        {
            string surfacePath = Path.Combine(Application.streamingAssetsPath, "AiNav", CurrentSceneName, surfaceId.ToString());

            if (!Directory.Exists(surfacePath))
            {
                Directory.CreateDirectory(surfacePath);
            }
            return surfacePath;
        }

        private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
        {
            if (SurfacesSceneScoped) return;
            CurrentSceneName = scene.name;
        }

    }
}
