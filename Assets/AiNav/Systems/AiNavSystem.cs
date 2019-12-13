using System.Collections.Generic;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;

namespace AiNav
{
    [AlwaysUpdateSystem]
    public class AiNavSystem : JobComponentSystem
    {
        public static AiNavSystem Instance => EcsWorld.Active.GetExistingSystem<AiNavSystem>();

        public delegate void OnSurfaceCreatedEvent(int surfaceId);
        public static OnSurfaceCreatedEvent OnSurfaceCreated = delegate { };

        public delegate void OnSurfaceDestroyedEvent(int surfaceId);
        public static OnSurfaceDestroyedEvent OnSurfaceDestroyed = delegate { };

        public SharedMeshDb SharedMeshDb { get; private set; }
        public NativeHashMap<int, BlobAssetReference<MeshSourceData>> MeshDatas { get; private set; }

        private Dictionary<int, SurfaceController> Surfaces = new Dictionary<int, SurfaceController>();

        public void SetSurface(SurfaceControllerConfig config)
        {
            RemoveSurface(config.SurfaceId);

            if (!Surfaces.TryGetValue(config.SurfaceId, out SurfaceController surfaceManager))
            {
                surfaceManager = new SurfaceController(config, this);
                Surfaces[config.SurfaceId] = surfaceManager;

                OnSurfaceCreated?.Invoke(config.SurfaceId);
            }
        }

        public void RemoveSurface(int id)
        {
            if (Surfaces.TryGetValue(id, out SurfaceController builderSystem))
            {
                Surfaces.Remove(id);
                builderSystem.OnDestroy();
                OnSurfaceDestroyed?.Invoke(id);
            }
        }

        public bool HasSurface(int id)
        {
            return Surfaces.ContainsKey(id);
        }

        public SurfaceController GetSurfaceController(int id)
        {
            if (Surfaces.TryGetValue(id, out SurfaceController controller))
            {
                return controller;
            } else
            {
                return null;
            }
        }

        public bool TryGetSharedBlobRef(int id, out BlobAssetReference<MeshSourceData> data)
        {
            return MeshDatas.TryGetValue(id, out data);
        }

        protected override void OnCreate()
        {
            base.OnCreate();
            SharedMeshDb = SharedMeshDb.LoadDb();
            MeshDatas = SharedMeshDb.LoadSharedMeshDatas();
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();
            foreach (var builderSystem in Surfaces.Values)
            {
                builderSystem.OnDestroy();
            }
            Surfaces.Clear();

            if (MeshDatas.IsCreated) MeshDatas.Dispose();
        }

        protected override JobHandle OnUpdate(JobHandle inputDeps)
        {
            foreach (var builderSystem in Surfaces.Values)
            {
                inputDeps = builderSystem.OnUpdate(inputDeps);
            }
            return inputDeps;
        }

    }
}
