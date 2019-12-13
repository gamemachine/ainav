using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using Unity.Collections;
using Unity.Mathematics;

namespace AiNav
{
    public class NavMeshBuilder 
    {
        public NavMeshBuildSettings BuildSettings { get; private set; }
        public NavAgentSettings AgentSettings { get; private set; }
        public DtBoundingBox HeightBounds { get; private set; }
        public NavMeshBuildResult BuildResult;
        public Dictionary<int2, NavMeshTile> Tiles { get; private set; } = new Dictionary<int2, NavMeshTile>();
        private HashSet<int2> TilesToBuild = new HashSet<int2>();
        private List<NavMeshBuildInput> InputsFromNativeList = new List<NavMeshBuildInput>();

        public bool HasTilesToBuild
        {
            get
            {
                return TilesToBuild.Count > 0;
            }
        }

        public NavMeshBuilder(NavMeshBuildSettings buildSettings, NavAgentSettings agentSettings)
        {
            BuildSettings = buildSettings;
            AgentSettings = agentSettings;
        }

        public void ClearTilesToBuild()
        {
            TilesToBuild.Clear();
        }

        public void MarkDirty(DtBoundingBox boundingBox)
        {
            var tiles = NavMeshBuildUtils.GetOverlappingTiles(BuildSettings, boundingBox);
            foreach (var tileCoord in tiles)
            {
                TilesToBuild.Add(tileCoord);
            }
        }

        public void GetDirtyTileBounds(NativeList<NavMeshTileBounds> dirty)
        {
            foreach (var tileCoord in TilesToBuild)
            {
                DtBoundingBox bounds = NavMeshBuildUtils.CalculateTileBoundingBox(BuildSettings, tileCoord);
                NavMeshTileBounds tileBounds = new NavMeshTileBounds(tileCoord, bounds);
                dirty.Add(tileBounds);
            }
        }

        public void GetDirtyTileBounds(NativeQueue<NavMeshTileBounds> dirty)
        {
            foreach (var tileCoord in TilesToBuild)
            {
                DtBoundingBox bounds = NavMeshBuildUtils.CalculateTileBoundingBox(BuildSettings, tileCoord);
                NavMeshTileBounds tileBounds = new NavMeshTileBounds(tileCoord, bounds);
                dirty.Enqueue(tileBounds);
            }
        }

        public void GetDirtyTileBounds(List<NavMeshTileBounds> dirty)
        {
            foreach (var tileCoord in TilesToBuild)
            {
                DtBoundingBox bounds = NavMeshBuildUtils.CalculateTileBoundingBox(BuildSettings, tileCoord);
                NavMeshTileBounds tileBounds = new NavMeshTileBounds(tileCoord, bounds);
                dirty.Add(tileBounds);
            }
        }
        public unsafe bool BuildInputPerTile(NativeList<NavMeshBuildInput> inputs)
        {
            InputsFromNativeList.Clear();
            for (int i=0;i<inputs.Length;i++)
            {
                InputsFromNativeList.Add(inputs[i]);
            }

            return BuildInputPerTile(InputsFromNativeList);
        }

        public unsafe bool BuildSingleTile(NavMeshBuildInput single)
        {
            List<NavMeshBuildInput> inputs = new List<NavMeshBuildInput>() { single };
            return BuildInputPerTile(inputs);
        }

        public unsafe bool BuildInputPerTile(List<NavMeshBuildInput> inputs)
        {
            HeightBounds = default;
            Tiles.Clear();
            TilesToBuild.Clear();

            BuildResult = new NavMeshBuildResult();

            NormalizeInputHeights(inputs);
            SetGlobalBounds(inputs);

            foreach (NavMeshBuildInput input in inputs)
            {
                int result = BuildTile(input.TileBounds.Coord, BuildSettings, AgentSettings, input, 1, out NavMeshTile tile);

                if (result == 0)
                {
                    Tiles[input.TileBounds.Coord] = tile;
                    BuildResult.TilesBuilt++;
                }
                else if (result != 110)
                {
                    BuildResult.Result = result;
                    return false;
                }
            }

            BuildResult.Result = 0;

            return true;
        }

        public unsafe bool BuildAllFromSingleInput(NavMeshBuildInput single)
        {
            HeightBounds = default;
            Tiles.Clear();
            TilesToBuild.Clear();

            List<NavMeshBuildInput> inputs = new List<NavMeshBuildInput>() { single };
            BuildResult = new NavMeshBuildResult();
            
            NormalizeInputHeights(inputs);
            SetGlobalBounds(inputs);

            foreach (NavMeshBuildInput input in inputs)
            {
                var tiles = NavMeshBuildUtils.GetOverlappingTiles(BuildSettings, input.TileBounds.Bounds);
                foreach (var tileCoord in tiles)
                {
                    int result = BuildTile(tileCoord, BuildSettings, AgentSettings, input, 1, out NavMeshTile tile);
                    if (result == 0)
                    {
                        Tiles[tileCoord] = tile;
                        BuildResult.TilesBuilt++;
                    }
                    else if (result != 110)
                    {
                        BuildResult.Result = result;
                        return false;
                    }
                }
            }

            BuildResult.Result = 0;

            return true;
        }

        private void NormalizeInputHeights(List<NavMeshBuildInput> inputs)
        {
            float minimumHeight = float.MaxValue;
            float maximumHeight = float.MinValue;

            for (int i = 0; i < inputs.Count; i++)
            {
                NavMeshBuildInput input = inputs[i];
                maximumHeight = Math.Max(maximumHeight, input.TileBounds.Bounds.max.y);
                minimumHeight = Math.Min(minimumHeight, input.TileBounds.Bounds.min.y);
            }

            for (int i = 0; i < inputs.Count; i++)
            {
                NavMeshBuildInput input = inputs[i];
                input.TileBounds.Bounds.min.y = minimumHeight;
                input.TileBounds.Bounds.max.y = maximumHeight;
                inputs[i] = input;
            }
        }

        private void SetGlobalBounds(List<NavMeshBuildInput> inputs)
        {
            HeightBounds = inputs[0].TileBounds.Bounds;
            foreach (NavMeshBuildInput input in inputs)
            {
                HeightBounds = DtBoundingBox.Merge(HeightBounds, input.TileBounds.Bounds);
            }
        }

        private unsafe int BuildTile(int2 tileCoordinate, NavMeshBuildSettings buildSettings, NavAgentSettings agentSettings,
            NavMeshBuildInput buildInput, long buildTimeStamp, out NavMeshTile meshTile)
        {
            
            meshTile = null;

            if (buildInput.AreasLength != buildInput.IndicesLength / 3)
            {
                return -1001;
            }

            if (buildInput.VerticesLength <= 0 || buildInput.IndicesLength <= 0)
            {
                return -1000;
            }

            DtBoundingBox tileBoundingBox = NavMeshBuildUtils.CalculateTileBoundingBox(buildSettings, tileCoordinate);
            NavMeshBuildUtils.SnapBoundingBoxToCellHeight(buildSettings, ref tileBoundingBox);

            tileBoundingBox.min.y = HeightBounds.min.y;
            tileBoundingBox.max.y = HeightBounds.max.y;

            IntPtr builder = Navigation.NavMesh.CreateBuilder();

            DtBuildSettings internalBuildSettings = new DtBuildSettings
            {
                // Tile settings
                BoundingBox = tileBoundingBox,
                TilePosition = tileCoordinate,
                TileSize = buildSettings.TileSize,

                // General build settings
                CellHeight = buildSettings.CellHeight,
                CellSize = buildSettings.CellSize,
                RegionMinArea = buildSettings.MinRegionArea,
                RegionMergeArea = buildSettings.RegionMergeArea,
                EdgeMaxLen = buildSettings.MaxEdgeLen,
                EdgeMaxError = buildSettings.MaxEdgeError,
                DetailSampleDist = buildSettings.DetailSamplingDistance,
                DetailSampleMaxError = buildSettings.MaxDetailSamplingError,

                // Agent settings
                AgentHeight = agentSettings.Height,
                AgentRadius = agentSettings.Radius,
                AgentMaxClimb = agentSettings.MaxClimb,
                AgentMaxSlope = agentSettings.MaxSlope
            };

            Navigation.NavMesh.SetSettings(builder, new IntPtr(&internalBuildSettings));

            IntPtr buildResultPtr = Navigation.NavMesh.Build2(builder, buildInput.Vertices, buildInput.VerticesLength, buildInput.Indices, buildInput.IndicesLength, buildInput.Areas);
            
            DtGeneratedData* generatedDataPtr = (DtGeneratedData*)buildResultPtr;

            if (generatedDataPtr->Success && generatedDataPtr->NavmeshDataLength > 0)
            {
                meshTile = new NavMeshTile();

                // Copy the generated navigationMesh data
                meshTile.Data = new byte[generatedDataPtr->NavmeshDataLength + sizeof(long)];
                Marshal.Copy(generatedDataPtr->NavmeshData, meshTile.Data, 0, generatedDataPtr->NavmeshDataLength);

                // Append time stamp
                byte[] timeStamp = BitConverter.GetBytes(buildTimeStamp);
                for (int i = 0; i < timeStamp.Length; i++)
                    meshTile.Data[meshTile.Data.Length - sizeof(long) + i] = timeStamp[i];
            }

            int error = generatedDataPtr->Error;
            Navigation.NavMesh.DestroyBuilder(builder);

            return error;

        }

    }
}
