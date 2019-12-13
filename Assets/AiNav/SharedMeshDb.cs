using System;
using System.Collections.Generic;
using Unity.Collections;
using Unity.Entities;
using UnityEngine;

namespace AiNav
{
    [CreateAssetMenu(menuName = @"AiNav/SharedMeshDb")]
    public class SharedMeshDb : ScriptableObject
    {
        public string Name;
        public Mesh Mesh;

        [Serializable]
        public class SharedMesh
        {
            public int Id;
            public string Name;
            public Mesh Mesh;
        }

        [SerializeField]
        public List<SharedMesh> Meshes = new List<SharedMesh>();

        public static SharedMeshDb LoadDb()
        {
            return Resources.Load(typeof(SharedMeshDb).Name, typeof(SharedMeshDb)) as SharedMeshDb;
        }

        public int GetIdByName(string name)
        {
            for (int i = 0; i < Meshes.Count; i++)
            {
                var mesh = Meshes[i];
                if (mesh.Name == name)
                {
                    return mesh.Id;
                }
            }
            throw new ArgumentException("Invalid name");
        }

        public void AddMesh()
        {
            if (Mesh == null)
            {
                return;
            }

            int nextId = 0;
            SharedMesh mesh;

            for (int i=0;i<Meshes.Count;i++)
            {
                mesh = Meshes[i];
                if (mesh.Id > nextId)
                {
                    nextId = mesh.Id;
                }
            }
            nextId++;

            mesh = new SharedMesh { Id = nextId, Name = Name, Mesh = Mesh };
            Meshes.Add(mesh);
            Mesh = null;
            Name = null;

#if UNITY_EDITOR
            UnityEditor.EditorUtility.SetDirty(this);
#endif

        }

        public NativeHashMap<int, BlobAssetReference<MeshSourceData>> LoadSharedMeshDatas()
        {
            var blobs = NavMeshStoreSystem.Instance.LoadSources(-1);
            if (blobs.Length != Meshes.Count)
            {
                blobs.Dispose();
                throw new InvalidOperationException("Saved mesh count does not equal db mesh count");
            }

            NativeHashMap<int, BlobAssetReference<MeshSourceData>> blobMap = new NativeHashMap<int, BlobAssetReference<MeshSourceData>>(blobs.Length, Allocator.Persistent);
            
            for(int i=0;i<blobs.Length;i++)
            {
                var blob = blobs[i];
                var sharedMesh = Meshes[i];
                blobMap.TryAdd(sharedMesh.Id, blob);
            }

            blobs.Dispose();
            return blobMap;
        }

        public void Save()
        {
            NativeArray<BlobAssetReference<MeshSourceData>> blobs = new NativeArray<BlobAssetReference<MeshSourceData>>(Meshes.Count, Allocator.Temp);
            for(int i=0;i<Meshes.Count;i++)
            {
                SharedMesh sharedMesh = Meshes[i];
                var blob = MeshSourceData.Create(sharedMesh.Mesh);
                blobs[i] = blob;
            }
            NavMeshStoreSystem.Instance.SaveSources(blobs, -1);
        }

        public void AddPrimitives()
        {
            SharedMesh sm = new SharedMesh { Id = (int)PrimitiveType.Capsule + 1, Mesh = MeshSourceData.CreateMesh(PrimitiveType.Capsule), Name = PrimitiveType.Capsule.ToString() };
            Meshes.Add(sm);
            sm = new SharedMesh { Id = (int)PrimitiveType.Cube + 1, Mesh = MeshSourceData.CreateMesh(PrimitiveType.Cube), Name = PrimitiveType.Cube.ToString() };
            Meshes.Add(sm);
            sm = new SharedMesh { Id = (int)PrimitiveType.Cylinder + 1, Mesh = MeshSourceData.CreateMesh(PrimitiveType.Cylinder), Name = PrimitiveType.Cylinder.ToString() };
            Meshes.Add(sm);
            sm = new SharedMesh { Id = (int)PrimitiveType.Sphere + 1, Mesh = MeshSourceData.CreateMesh(PrimitiveType.Sphere), Name = PrimitiveType.Sphere.ToString() };
            Meshes.Add(sm);

#if UNITY_EDITOR
            UnityEditor.EditorUtility.SetDirty(this);
#endif

        }
    }
}
