using Unity.Mathematics;

namespace AiNav
{
    public struct NavMeshTileBounds
    {
        public int2 Coord;
        public DtBoundingBox Bounds;

        public NavMeshTileBounds(int2 coord, DtBoundingBox bounds)
        {
            Coord = coord;
            Bounds = bounds;
        }
    }
}
