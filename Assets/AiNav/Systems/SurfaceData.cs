using Unity.Collections;
using Unity.Entities;
using UnityEngine;

namespace AiNav
{
    // this gets a bit confusing with the shared/non shared stuff so abstracted it out here
    public static class SurfaceData
    {
        private const string Shared = "Shared";
        private const string NonShared = "NonShared";

        public static void SaveScene(int surfaceId)
        {
            SaveSceneMeshSources(surfaceId);
            SaveSceneFilters(surfaceId);
        }

        public static void Load(SurfaceController controller)
        {
            int nonShared = LoadNonSharedMeshSources(controller);
            int shared = LoadSharedMeshSources(controller);
            int filters = LoadFilters(controller);
        }

        private static int LoadFilters(SurfaceController controller)
        {
            int surfaceId = controller.Config.SurfaceId;

            if (!NavMeshStoreSystem.Instance.Exists<BoxFilter>(surfaceId))
            {
                return 0;
            }

            int boxFiltersLength = NavMeshStoreSystem.Instance.GetArrayLength<BoxFilter>(surfaceId);
            NativeArray<BoxFilter> boxFilters = new NativeArray<BoxFilter>(boxFiltersLength, Allocator.Temp);
            NavMeshStoreSystem.Instance.LoadArray(boxFilters, surfaceId);

            for (int i = 0; i < boxFilters.Length; i++)
            {
                controller.AddBoxFilter(boxFilters[i]);

            }

            boxFilters.Dispose();
            return boxFiltersLength;
        }

        private static int LoadSharedMeshSources(SurfaceController controller)
        {
            int surfaceId = controller.Config.SurfaceId;
            if (!NavMeshStoreSystem.Instance.Exists<MeshSourceInfo>(surfaceId, Shared))
            {
                return 0;
            }
            int infosLength = NavMeshStoreSystem.Instance.GetArrayLength<MeshSourceInfo>(surfaceId, Shared);
            NativeArray<MeshSourceInfo> infos = new NativeArray<MeshSourceInfo>(infosLength, Allocator.Temp);
            NavMeshStoreSystem.Instance.LoadArray(infos, surfaceId, Shared);

            for (int i = 0; i < infos.Length; i++)
            {
                MeshSource source = new MeshSource { Info = infos[i] };
                controller.AddMeshSource(ref source);
            }
            infos.Dispose();
            return infosLength;
        }

        private static int LoadNonSharedMeshSources(SurfaceController controller)
        {
            int surfaceId = controller.Config.SurfaceId;
            if (!NavMeshStoreSystem.Instance.Exists<MeshSourceInfo>(surfaceId, NonShared))
            {
                return 0;
            }

            NativeArray<BlobAssetReference<MeshSourceData>> datas = NavMeshStoreSystem.Instance.LoadSources(surfaceId);
            int infosLength = NavMeshStoreSystem.Instance.GetArrayLength<MeshSourceInfo>(surfaceId, NonShared);
            NativeArray<MeshSourceInfo> infos = new NativeArray<MeshSourceInfo>(infosLength, Allocator.Temp);
            NavMeshStoreSystem.Instance.LoadArray(infos, surfaceId, NonShared);

            for (int i = 0; i < infos.Length; i++)
            {
                MeshSource source = new MeshSource { Info = infos[i], Value = datas[i] };
                controller.AddMeshSource(ref source);
            }
            infos.Dispose();
            datas.Dispose();
            return infosLength;
        }

        private static void SaveSharedMeshSources(int surfaceId, NativeList<MeshSource> meshSources)
        {
            NativeArray<MeshSourceInfo> infos = new NativeArray<MeshSourceInfo>(meshSources.Length, Allocator.Temp);

            for (int i = 0; i < meshSources.Length; i++)
            {
                var meshSource = meshSources[i];
                infos[i] = meshSource.Info;
            }
            NavMeshStoreSystem.Instance.SaveArray(infos, surfaceId, Shared);
            infos.Dispose();
        }

        private static void SaveNonSharedMeshSources(int surfaceId, NativeList<MeshSource> meshSources)
        {
            NativeArray<BlobAssetReference<MeshSourceData>> blobs = new NativeArray<BlobAssetReference<MeshSourceData>>(meshSources.Length, Allocator.Temp);
            NativeArray<MeshSourceInfo> infos = new NativeArray<MeshSourceInfo>(meshSources.Length, Allocator.Temp);

            for (int i = 0; i < meshSources.Length; i++)
            {
                var meshSource = meshSources[i];
                infos[i] = meshSource.Info;
                blobs[i] = meshSource.Value;
            }
            NavMeshStoreSystem.Instance.SaveSources(blobs, surfaceId);
            NavMeshStoreSystem.Instance.SaveArray(infos, surfaceId, NonShared);

            for (int i = 0; i < meshSources.Length; i++)
            {
                var meshSource = meshSources[i];
                meshSource.Value.Dispose();//.Release();
            }
            blobs.Dispose();
            infos.Dispose();
        }

        private static void SaveSceneMeshSources(int surfaceId)
        {
            NativeList<MeshSource> nonShared = new NativeList<MeshSource>(Allocator.Temp);
            NativeList<MeshSource> shared = new NativeList<MeshSource>(Allocator.Temp);

            var authors = Object.FindObjectsOfType<MeshSourceAuthorBase>();
            Debug.LogFormat("Found {0} MeshSource authors", authors.Length);

            foreach(var author in authors)
            {
                author.AppendSources(surfaceId, nonShared, shared);
            }

            SaveSharedMeshSources(surfaceId, shared);
            SaveNonSharedMeshSources(surfaceId, nonShared);

            shared.Dispose();
            nonShared.Dispose();
        }

        private static void SaveSceneFilters(int surfaceId)
        {
            NativeList<BoxFilter> boxFilters = new NativeList<BoxFilter>(Allocator.Temp);
            foreach (var filter in Object.FindObjectsOfType<BoxFilterAuthoring>())
            {
                boxFilters.Add(filter.BoxFilter);
            }
            NavMeshStoreSystem.Instance.SaveArray(boxFilters.AsArray(), surfaceId);
            Debug.LogFormat("found {0} filter authors", boxFilters.Length);
            boxFilters.Dispose();
        }

    }
}
