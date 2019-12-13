using AiNav.Collections;
using System;
using System.Collections.Generic;
using Unity.Collections;
using Unity.Mathematics;

namespace AiNav
{
    public class NavMeshBuildUtils
    {
        /// <summary>
        /// Check which tiles overlap a given bounding box
        /// </summary>
        /// <param name="settings"></param>
        /// <param name="boundingBox"></param>
        /// <returns></returns>
        public static List<int2> GetOverlappingTiles(NavMeshBuildSettings settings, DtBoundingBox boundingBox)
        {
            List<int2> ret = new List<int2>();
            float tcs = settings.TileSize * settings.CellSize;
            float2 start = boundingBox.min.xz / tcs;
            float2 end = boundingBox.max.xz / tcs;

            int2 startTile = new int2(
                (int)Math.Floor(start.x),
                (int)Math.Floor(start.y));
            int2 endTile = new int2(
                (int)Math.Ceiling(end.x),
                (int)Math.Ceiling(end.y));

            for (int y = startTile.y; y < endTile.y; y++)
            {
                for (int x = startTile.x; x < endTile.x; x++)
                {
                    ret.Add(new int2(x, y));
                }
            }
            return ret;
        }

        /// <summary>
        /// Snaps a <see cref="DtBoundingBox"/>'s height according to the given <see cref="NavMeshBuildSettings"/>
        /// </summary>
        /// <param name="settings">The build settings</param>
        /// <param name="boundingBox">Reference to the bounding box to snap</param>
        public static void SnapBoundingBoxToCellHeight(NavMeshBuildSettings settings, ref DtBoundingBox boundingBox)
        {
            // Snap Y to tile height to avoid height differences between tiles
            boundingBox.min.y = (float)Math.Floor(boundingBox.min.y / settings.CellHeight) * settings.CellHeight;
            boundingBox.max.y = (float)Math.Ceiling(boundingBox.max.y / settings.CellHeight) * settings.CellHeight;
        }

        /// <summary>
        /// Calculates X-Z span for a navigation mesh tile. The Y-axis will span from <see cref="float.MinValue"/> to <see cref="float.MaxValue"/>
        /// </summary>
        public static DtBoundingBox CalculateTileBoundingBox(NavMeshBuildSettings settings, int2 tileCoord)
        {
            float tcs = settings.TileSize * settings.CellSize;
            float2 tileMin = new float2(tileCoord.x * tcs, tileCoord.y * tcs);
            float2 tileMax = tileMin + new float2(tcs);

            DtBoundingBox boundingBox = default;
            boundingBox.min.x = tileMin.x;
            boundingBox.min.z = tileMin.y;
            boundingBox.max.x = tileMax.x;
            boundingBox.max.z = tileMax.y;
            boundingBox.min.y = float.MinValue;
            boundingBox.max.y = float.MaxValue;

            return boundingBox;
        }

    }
}
