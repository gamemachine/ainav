using Unity.Collections;
using Unity.Mathematics;

namespace AiNav
{
    public struct MeshSourceMap
    {
        private static int NextId = 1;

        public NativeHashMap<int, MeshSource> IdIndex;
        public NativeHashMap<int, int> CustomIdToIdIndex;
        public NativeMultiHashMap<int2, int> TileSources;
        public NavMeshBuildSettings BuildSettings;

        public MeshSourceMap(NavMeshBuildSettings buildSettings)
        {
            BuildSettings = buildSettings;
            TileSources = new NativeMultiHashMap<int2, int>(256, Allocator.Persistent);
            IdIndex = new NativeHashMap<int, MeshSource>(256, Allocator.Persistent);
            CustomIdToIdIndex = new NativeHashMap<int, int>(256, Allocator.Persistent);
        }

        public void Dispose()
        {
            if (TileSources.IsCreated)
            {
                TileSources.Dispose();

                var values = IdIndex.GetValueArray(Allocator.Temp);
                for (int i = 0; i < values.Length; i++)
                {
                    values[i].Dispose();
                }
                
                IdIndex.Dispose();
                CustomIdToIdIndex.Dispose();
            }
        }

        public void GetCounts(out int sourcesCount, out int tileSourcesCount)
        {
            sourcesCount = IdIndex.Length;
            tileSourcesCount = TileSources.Length;
        }

        public NativeList<MeshSource> GetSources(int2 tileCoord)
        {
            NativeList<MeshSource> sources = new NativeList<MeshSource>(Allocator.Temp);
            int sourceId;
            NativeMultiHashMapIterator<int2> iterator;
            bool found = TileSources.TryGetFirstValue(tileCoord, out sourceId, out iterator);

            while (found)
            {
                if (IdIndex.TryGetValue(sourceId, out MeshSource source))
                {
                    sources.Add(source);
                }
                

                found = TileSources.TryGetNextValue(out sourceId, ref iterator);
            }

            return sources;
        }

        public bool TryGetByCustomData(int customData, out MeshSource source)
        {
            source = default;
            if (CustomIdToIdIndex.TryGetValue(customData, out int id))
            {
                if (IdIndex.TryGetValue(id, out source))
                {
                    return true;
                }
            }
            return false;
        }

        public void Add(MeshSource source)
        {
            if (IdIndex.TryGetValue(source.Id, out MeshSource existing))
            {
                existing.Dispose();
                IdIndex.Remove(source.Id);
                CustomIdToIdIndex.Remove(source.Info.CustomData);
            }

            IdIndex.TryAdd(source.Id, source);
            CustomIdToIdIndex.TryAdd(source.Info.CustomData, source.Id);

            NativeList<int2> tileCoords = NativeBuildUtitls.GetOverlappingTiles(BuildSettings, source.Info.Bounds);
            for(int i=0;i<tileCoords.Length;i++)
            {
                int2 coord = tileCoords[i];
                TileSources.Add(coord, source.Id);
            }
            tileCoords.Dispose();
        }

        public bool Remove(int id)
        {
            if (!IdIndex.TryGetValue(id, out MeshSource source))
            {
                return false;
            }

            NativeList<int2> tileCoords = NativeBuildUtitls.GetOverlappingTiles(BuildSettings, source.Info.Bounds);
            for (int i = 0; i < tileCoords.Length; i++)
            {
                int2 coord = tileCoords[i];

                int sourceId;
                NativeMultiHashMapIterator<int2> iterator;
                bool found = TileSources.TryGetFirstValue(coord, out sourceId, out iterator);

                while (found && sourceId == id)
                {
                    TileSources.Remove(iterator);
                    found = TileSources.TryGetNextValue(out sourceId, ref iterator);
                }
            }

            IdIndex.Remove(source.Id);
            CustomIdToIdIndex.Remove(source.Info.CustomData);
            source.Dispose();

            return true;
        }
    }
}
