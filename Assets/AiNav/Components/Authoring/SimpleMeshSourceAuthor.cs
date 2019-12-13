using System;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

namespace AiNav
{
    public class SimpleMeshSourceAuthor : MeshSourceAuthorBase
    {
        public int SurfaceId = 1;
        [SerializeField, Layer]
        private int Layer = 31;
        public MeshSource Current;
        public bool AddOnSceneCollect;
        public byte Area = DtArea.WALKABLE;
        public byte Flag = 1;
        public bool Shared;

        [SerializeField]
        private PrimitiveType SharedPrimitiveType;
        [SerializeField]
        private int CustomSharedMeshId;
        [SerializeField]
        private MeshFilter MeshFilter;
       
        public int SharedMeshId
        {
            get
            {
                if (!Shared) return 0;
                int sharedId = CustomSharedMeshId;
                if (sharedId == 0)
                {
                    sharedId = (int)SharedPrimitiveType + 1;
                }
                return sharedId;
            }
        }
        
        public void RemoveFromSurface()
        {
            if (Current.Id <= 0) return;
            var controller = SurfaceController.Current(SurfaceId);
            if (controller != null)
            {
                if (controller.RemoveMeshSource(Current.Id))
                {
                    controller.MarkDirty(Current.Info.Bounds);
                    Current = default;
                }
            }
        }

        public void AddToSurface()
        {
            var controller = SurfaceController.Current(SurfaceId);
            if (controller != null)
            {
                Current = GetMeshSource();
                controller.AddMeshSource(ref Current);
                controller.MarkDirty(Current.Info.Bounds);
            }
        }

        public override void AppendSources(int surfaceId, NativeList<MeshSource> nonShared, NativeList<MeshSource> shared)
        {
            if (!AddOnSceneCollect || surfaceId != SurfaceId) return;

            if (Shared)
            {
                shared.Add(GetMeshSource());
            } else
            {
                nonShared.Add(GetMeshSource());
            }
        }

        public MeshSource GetMeshSource()
        {
            float4x4 localToWorld = float4x4.TRS(transform.position, transform.rotation, transform.localScale);
            if (Shared)
            {
                int sharedId = SharedMeshId;
                var navSystem = EcsWorld.Active.GetExistingSystem<AiNavSystem>();
                if (navSystem.TryGetSharedBlobRef(sharedId, out BlobAssetReference<MeshSourceData> data))
                {
                    return MeshSource.CreateShared(localToWorld, data, Layer, Area, Flag, sharedId);
                } else
                {
                    throw new InvalidOperationException(string.Format("shared mesh not found {0}", sharedId));
                }
            } else
            {
                return MeshSource.Create(localToWorld, MeshFilter.sharedMesh, Layer, Area, Flag);
            }
            
        }
    }
}
