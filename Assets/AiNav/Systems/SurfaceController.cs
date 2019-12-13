using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Physics;

namespace AiNav
{
    public partial class SurfaceController
    {
        public delegate void OnBuildCompletedEvent(int surfaceId);
        public OnBuildCompletedEvent OnBuildCompleted = delegate { };

        public SurfaceControllerConfig Config { get; private set; }
        public AiNavMesh NavMesh { get; private set; }
        public NavMeshBuilder Builder { get; private set; }
        public CrowdController CrowdController { get; private set; }
        public bool Building { get; private set; }

        private NativeList<PhysicsCollider> Colliders;
        private NativeQueue<NavMeshTileBounds> TilesToBuild;
        private NativeQueue<NavMeshBuildInput> BuildInputs;
        private NativeList<BoxFilter> BoxFilters;
        private NativeArray<int> CurrentTileStatus;
        private NativeArray<int> TilesSavedStatus;
        private MeshSourceMap MeshSourceMap;

        private List<NavMeshNativeInputBuilder> CurrentInputBuilders = new List<NavMeshNativeInputBuilder>();
        private Dictionary<int2, NavMeshTile> Tiles = new Dictionary<int2, NavMeshTile>();
        private Dictionary<int2, NavMeshTile> RebuiltTiles = new Dictionary<int2, NavMeshTile>();

        private IntPtr TilesPtr;
        private GCHandle TilesHandle;

        private IntPtr RebuiltTilesPtr;
        private GCHandle RebuiltTilesHandle;

        private System.Diagnostics.Stopwatch Watch;
        public NativeList<JobHandle> HandlesToWaitFor;
        private AiNavSystem AiNavSystem;
        private int NextMeshSourceId = 1;

        public static SurfaceController Current(int surfaceId)
        {
            var navSystem = EcsWorld.Active.GetExistingSystem<AiNavSystem>();
            if (navSystem != null)
            {
                return navSystem.GetSurfaceController(surfaceId);
            }
            return null;
        }

        public SurfaceController(SurfaceControllerConfig config, AiNavSystem system)
        {
            Config = config;
            AiNavSystem = system;

            Builder = new NavMeshBuilder(Config.BuildSettings, Config.AgentSettings);

            Colliders = new NativeList<PhysicsCollider>(Allocator.Persistent);
            TilesToBuild = new NativeQueue<NavMeshTileBounds>(Allocator.Persistent);
            BuildInputs = new NativeQueue<NavMeshBuildInput>(Allocator.Persistent);
            CurrentTileStatus = new NativeArray<int>(1, Allocator.Persistent);
            BoxFilters = new NativeList<BoxFilter>(Allocator.Persistent);
            MeshSourceMap = new MeshSourceMap(Builder.BuildSettings);
            TilesSavedStatus = new NativeArray<int>(1, Allocator.Persistent);

            TilesHandle = GCHandle.Alloc(Tiles);
            TilesPtr = GCHandle.ToIntPtr(TilesHandle);

            RebuiltTilesHandle = GCHandle.Alloc(RebuiltTiles);
            RebuiltTilesPtr = GCHandle.ToIntPtr(RebuiltTilesHandle);

            NavMesh = new AiNavMesh(Builder.BuildSettings.TileSize, Builder.BuildSettings.CellSize);

            bool loaded = NavMeshStoreSystem.Instance.LoadTiles(Config.SurfaceId, Tiles);

            foreach (NavMeshTile tile in Tiles.Values)
            {
                NavMesh.AddOrReplaceTile(tile.Data);
            }

            if (Config.CrowdEnabled)
            {
                CrowdController = new CrowdController(NavMesh, this);
            }

            HandlesToWaitFor = new NativeList<JobHandle>(Allocator.Persistent);

            SurfaceData.Load(this);
        }

        // Assigns unique id.  So if you add/remove multiple times make sure to use the same copy.
        public void AddMeshSource(ref MeshSource source)
        {
            if (Building) throw new InvalidOperationException("Building");

            if (source.Id == 0)
            {
                source.Id = NextMeshSourceId;
                NextMeshSourceId++;
            }

            MeshSourceMap.Add(source);
        }

        public bool RemoveMeshSource(int id)
        {
            if (Building) throw new InvalidOperationException("Building");

            return MeshSourceMap.Remove(id);
        }

        public bool TryGetMeshSourceByCustomData(int customData, out MeshSource source)
        {
            return MeshSourceMap.TryGetByCustomData(customData, out source);
        }

        public void AddBoxFilter(BoxFilter source)
        {
            if (Building) throw new InvalidOperationException("Building");
            BoxFilters.Add(source);
        }

       
        public void MarkDirty(DtBoundingBox bounds)
        {
            if (Building) throw new InvalidOperationException("Building");
            Builder.MarkDirty(bounds);
        }

        public void AddQueryDependency(JobHandle other)
        {
            HandlesToWaitFor.Add(other);
        }

        public void OnDestroy()
        {
            if (Watch != null)
            {
                Watch.Stop();
            }

            if (CrowdController != null)
            {
                CrowdController.OnDestroy();
            }


            NavMesh.Dispose();

            foreach (var inputBuilder in CurrentInputBuilders)
            {
                inputBuilder.Dispose();
            }
            CurrentInputBuilders.Clear();

            if (Colliders.IsCreated) Colliders.Dispose();
            if (TilesToBuild.IsCreated) TilesToBuild.Dispose();
            if (BuildInputs.IsCreated) BuildInputs.Dispose();
            if (CurrentTileStatus.IsCreated) CurrentTileStatus.Dispose();
            if (BoxFilters.IsCreated) BoxFilters.Dispose();
            if (HandlesToWaitFor.IsCreated) HandlesToWaitFor.Dispose();
            if (TilesSavedStatus.IsCreated) TilesSavedStatus.Dispose();
            MeshSourceMap.Dispose();

            RebuiltTilesHandle.Free();
            TilesHandle.Free();
        }

        public JobHandle OnUpdate(JobHandle inputDeps)
        {
            if (CrowdController != null)
            {
                inputDeps = CrowdController.OnUpdate(AiNavSystem, inputDeps);
            }


            if (!Building)
            {
                if (TilesSavedStatus[0] == 1)
                {
                    TilesSavedStatus[0] = 0;
                    OnBuildCompleted?.Invoke(Config.SurfaceId);
                }

                if (Builder.HasTilesToBuild)
                {
                    
                    Watch = System.Diagnostics.Stopwatch.StartNew();
                    Building = true;
                    Builder.GetDirtyTileBounds(TilesToBuild);
                    Builder.ClearTilesToBuild();
                    RebuiltTiles.Clear();
                    UnityEngine.Debug.LogFormat("Building {0} tiles", TilesToBuild.Count);
                }

                if (!Building)
                {
                    HandlesToWaitFor.Clear();
                }

                return inputDeps;
            }

            int status = CurrentTileStatus[0];

            // No tiles building
            if (status == 0)
            {
                for (int i = 0; i < Config.BatchSize; i++)
                {
                    if (TilesToBuild.TryDequeue(out NavMeshTileBounds tileBounds))
                    {
                        tileBounds.Bounds.min.y = -1024f;
                        tileBounds.Bounds.max.y = 1024f;
                        tileBounds.Bounds.Expand(2f);

                        NavMeshNativeInputBuilder inputBuilder = new NavMeshNativeInputBuilder(tileBounds);

                        if (Config.IncludeUnityPhysicsGeometry)
                        {
                            CollectGeometryJob collectGeometryJob = new CollectGeometryJob
                            {
                                IncludeMask = Config.IncludeMask,
                                BoxFilters = BoxFilters,
                                TileBounds = tileBounds,
                                InputBuilder = inputBuilder,
                                GeometryFilter = Config.GeometryFilter
                            };
                            inputDeps = collectGeometryJob.ScheduleSingle(AiNavSystem, inputDeps);
                        }
                        

                        CollectFromMeshSourcesJob collectFromMeshSources = new CollectFromMeshSourcesJob
                        {
                            IncludeMask = Config.IncludeMask,
                            MeshSourceMap = MeshSourceMap,
                            TileBounds = tileBounds,
                            InputBuilder = inputBuilder,
                            SharedMeshSources = AiNavSystem.MeshDatas
                        };
                        inputDeps = collectFromMeshSources.Schedule(inputDeps);

                        CurrentInputBuilders.Add(inputBuilder);
                        CurrentTileStatus[0] = 1;
                    }
                }

                if (CurrentInputBuilders.Count == 0)
                {
                    if (Building)
                    {
                        CurrentTileStatus[0] = 0;

                        if (RebuiltTiles.Count > 0)
                        {
                            var handle = JobHandle.CombineDependencies(HandlesToWaitFor);
                            handle.Complete();

                            foreach (NavMeshTile rebuiltTile in RebuiltTiles.Values)
                            {
                                Tiles[rebuiltTile.Coord] = rebuiltTile;
                                NavMesh.AddOrReplaceTile(rebuiltTile.Data);
                            }

                            SaveTilesJob saveTilesJob = new SaveTilesJob
                            {
                                TilesSavedStatus = TilesSavedStatus,
                                SurfaceId = Config.SurfaceId,
                                TilesPtr = TilesPtr
                            };
                            inputDeps = saveTilesJob.Schedule(inputDeps);

                        }

                        Watch.Stop();
                        UnityEngine.Debug.LogFormat("Build finished in {0} tilecount {1}", Watch.ElapsedMilliseconds, RebuiltTiles.Count);
                        RebuiltTiles.Clear();
                        Building = false;
                    }
                }

                return inputDeps;
            }

            // collection done, build
            if (status == 1)
            {
                foreach (var inputBuilder in CurrentInputBuilders)
                {
                    if (inputBuilder.Vertices.Length > 0)
                    {
                        BuildInputs.Enqueue(inputBuilder.ToBuildInput());

                    }
                }

                BuildTileJob buildTileJob = new BuildTileJob
                {
                    AgentSettings = Builder.AgentSettings,
                    BuildSettings = Builder.BuildSettings,
                    BuildInputs = BuildInputs,
                    RebuiltTilesPtr = RebuiltTilesPtr,
                    SurfaceId = Config.SurfaceId
                };
                inputDeps = buildTileJob.Schedule(inputDeps);

                CurrentTileStatus[0] = 2;
                return inputDeps;
            }

            // Tile builds finished
            if (status == 2)
            {
                foreach (var inputBuilder in CurrentInputBuilders)
                {
                    inputBuilder.Dispose();
                }
                CurrentInputBuilders.Clear();
                CurrentTileStatus[0] = 0;
            }

            return inputDeps;
        }


    }
}
