using System;
using Unity.Collections;
using Unity.Mathematics;

namespace AiNav
{
    public static class NativeBuildUtitls
    {
        public static NativeList<int2> GetOverlappingTiles(NavMeshBuildSettings settings, DtBoundingBox boundingBox)
        {
            NativeList<int2> ret = new NativeList<int2>(Allocator.Temp);
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
    }
}
