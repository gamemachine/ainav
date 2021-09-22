using AiNav.Util;
using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using Unity.Burst;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Physics;
using Unity.Transforms;

namespace AiNav
{
    public partial class SurfaceController
    {
        [BurstCompile]
        unsafe struct CollectFromMeshSourcesJob : IJob
        {
            public CollisionFilter IncludeMask;
            public MeshSourceMap MeshSourceMap;
            public NavMeshTileBounds TileBounds;
            public NavMeshNativeInputBuilder InputBuilder;
            [ReadOnly]
            public NativeHashMap<int, BlobAssetReference<MeshSourceData>> SharedMeshSources;

            public void Execute()
            {
                NativeList<MeshSource> sources = MeshSourceMap.GetSources(TileBounds.Coord);
                for (int i = 0; i < sources.Length; i++)
                {
                    MeshSource source = sources[i];

                    if (!IncludeMask.HasLayer(source.Info.Layer))
                    {
                        continue;
                    }

                    NativeArray<int> indices;
                    NativeArray<float3> vertices;

                    if (source.Info.Shared)
                    {
                        if (!SharedMeshSources.TryGetValue(source.Info.SharedMeshId, out BlobAssetReference<MeshSourceData> data))
                        {
                            continue;
                        }

                        MeshSourceData.GetData(data, out indices, out vertices);
                        MeshSourceData.TransformInPlace(source.Info.TRS, vertices);
                        InputBuilder.Append(vertices, indices, source.Info.Area);
                    } else
                    {
                        MeshSourceData.GetData(source.Value, out indices, out vertices);
                        InputBuilder.Append(vertices, indices, source.Info.Area);
                    }
                    
                    vertices.Dispose();
                    indices.Dispose();
                }
            }
        }

        [BurstCompile]
        unsafe struct CollectGeometryJob : IJobForEach<PhysicsCollider, Translation, Rotation>
        {
            public CollisionFilter IncludeMask;
            public NativeList<BoxFilter> BoxFilters;
            public GeometryFilter GeometryFilter;
            public NavMeshTileBounds TileBounds;
            public NavMeshNativeInputBuilder InputBuilder;

            public void Execute([ReadOnly] ref PhysicsCollider collider, [ReadOnly] ref Translation translation, [ReadOnly] ref Rotation rotation)
            {
                if (!CollisionFilter.IsCollisionEnabled(IncludeMask, collider.ColliderPtr->Filter))
                {
                    return;
                }

                switch (collider.ColliderPtr->Type)
                {
                    case ColliderType.Box:
                    case ColliderType.Triangle:
                    case ColliderType.Quad:
                    case ColliderType.Cylinder:
                    case ColliderType.Convex:
                        HandleConvex((ConvexCollider*)collider.ColliderPtr, ref translation, ref rotation);
                        break;
                    case ColliderType.Sphere:
                        break;
                    case ColliderType.Capsule:
                        break;
                    case ColliderType.Mesh:
                        HandleMesh((MeshCollider*)collider.ColliderPtr, ref translation, ref rotation);
                        break;
                    case ColliderType.Compound:
                        break;
                    case ColliderType.Terrain:
                        HandleTerrain((TerrainCollider*)collider.ColliderPtr, ref translation, ref rotation);
                        break;
                    default:
                        break;
                }

            }

            private bool SetActiveBoundsFilters(DtBoundingBox colliderBounds)
            {
                bool hasFilter = false;
                for (int i = 0; i < BoxFilters.Length; i++)
                {
                    BoxFilter filter = BoxFilters[i];
                    if (DtBoundingBox.Intersects(ref filter.Bounds, ref colliderBounds))
                    {
                        filter.Active = true;
                        hasFilter = true;
                    }
                    else
                    {
                        filter.Active = false;
                    }
                    BoxFilters[i] = filter;
                }
                return hasFilter;
            }

            private void HandleConvex(ConvexCollider* collider, ref Translation translation, ref Rotation rotation)
            {
                RigidTransform colliderTransform = new RigidTransform(rotation.Value, translation.Value);
                Aabb aabb = collider->CalculateAabb(colliderTransform);
                DtBoundingBox colliderBox = new DtBoundingBox(aabb.Min, aabb.Max);

                if (DtBoundingBox.Intersects(ref TileBounds.Bounds, ref colliderBox))
                {

                    ConvexHullGeometryCollector collector = new ConvexHullGeometryCollector(InputBuilder, collider, colliderTransform, TileBounds.Bounds);
                    collector.Collect();
                }
            }

            private void HandleTerrain(TerrainCollider* collider, ref Translation translation, ref Rotation rotation)
            {
                RigidTransform colliderTransform = new RigidTransform(rotation.Value, translation.Value);
                Aabb aabb = collider->CalculateAabb(colliderTransform);
                DtBoundingBox colliderBox = new DtBoundingBox(aabb.Min, aabb.Max);

                if (DtBoundingBox.Intersects(ref TileBounds.Bounds, ref colliderBox))
                {
                    TerrainGeometryCollector collector = new TerrainGeometryCollector
                    {
                        InputBuilder = InputBuilder,
                        TerrainCollider = collider,
                        Transform = colliderTransform,
                        Bounds = TileBounds.Bounds,
                        GeometryFilter = GeometryFilter
                    };

                    if (SetActiveBoundsFilters(colliderBox))
                    {
                        collector.SetBoundsFilters(BoxFilters);
                    }
                    collector.Collect();
                }
            }

            private void HandleMesh(MeshCollider* collider, ref Translation translation, ref Rotation rotation)
            {
                RigidTransform colliderTransform = new RigidTransform(rotation.Value, translation.Value);
                Aabb aabb = collider->CalculateAabb(colliderTransform);
                DtBoundingBox colliderBox = new DtBoundingBox(aabb.Min, aabb.Max);

                if (DtBoundingBox.Intersects(ref TileBounds.Bounds, ref colliderBox))
                {
                    MeshGeometryCollector collector = new MeshGeometryCollector(InputBuilder, collider, colliderTransform, TileBounds.Bounds);
                    collector.Collect();
                }
            }

        }

        struct SaveTilesJob : IJob
        {
            public int SurfaceId;
            public NativeArray<int> TilesSavedStatus;
            [NativeDisableUnsafePtrRestriction]
            public IntPtr TilesPtr;

            public void Execute()
            {
                Dictionary<int2, NavMeshTile> tiles = GCHandle.FromIntPtr(TilesPtr).Target as Dictionary<int2, NavMeshTile>;
                var store = World.DefaultGameObjectInjectionWorld.GetExistingSystem<NavMeshStoreSystem>();
                store.SaveTiles(tiles, SurfaceId);
                TilesSavedStatus[0] = 1;
            }
        }

        struct BuildTileJob : IJob
        {
            public NavMeshBuildSettings BuildSettings;
            public NavAgentSettings AgentSettings;
            [NativeDisableUnsafePtrRestriction]
            public IntPtr RebuiltTilesPtr;
            public NativeQueue<NavMeshBuildInput> BuildInputs;
            public int SurfaceId;

            public void Execute()
            {
                Dictionary<int2, NavMeshTile> results = GCHandle.FromIntPtr(RebuiltTilesPtr).Target as Dictionary<int2, NavMeshTile>;
                NavMeshBuilder navMeshBuilder = new NavMeshBuilder(BuildSettings, AgentSettings);

                while (BuildInputs.TryDequeue(out NavMeshBuildInput buildInput))
                {
                    bool valid = navMeshBuilder.BuildSingleTile(buildInput);
                    if (valid && navMeshBuilder.BuildResult.IsValidBuild)
                    {
                        if (navMeshBuilder.Tiles.TryGetValue(buildInput.TileBounds.Coord, out NavMeshTile tile))
                        {
                            results[tile.Coord] = tile;
                        }
                        else
                        {
                            //UnityEngine.Debug.LogFormat("BuildResult no tile id:{0} bound:{1} vertices:{2} indices:{3}",
                            //    buildInput.TileBounds.Coord, buildInput.TileBounds.Bounds, buildInput.VerticesLength, buildInput.IndicesLength);
                        }
                    }
                    else
                    {
                        UnityEngine.Debug.LogFormat("BuildResult: {0}", navMeshBuilder.BuildResult);
                    }
                }
            }
        }


    }
}
