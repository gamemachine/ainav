using System;

namespace AiNav
{
    [Serializable]
    public struct NavMeshBuildSettings : IEquatable<NavMeshBuildSettings>
    {
        /// <summary>
        /// The Height of a grid cell in the navigation mesh building steps using heightfields. 
        /// A lower number means higher precision on the vertical axis but longer build times
        /// </summary>
        public float CellHeight;

        /// <summary>
        /// The Width/Height of a grid cell in the navigation mesh building steps using heightfields. 
        /// A lower number means higher precision on the horizontal axes but longer build times
        /// </summary>
        public float CellSize;

        /// <summary>
        /// Tile size used for Navigation mesh tiles, the final size of a tile is CellSize*TileSize
        /// </summary>
        public int TileSize;

        /// <summary>
        /// The minimum number of cells allowed to form isolated island areas
        /// </summary>
        public int MinRegionArea;

        /// <summary>
        /// Any regions with a span count smaller than this value will, if possible, 
        /// be merged with larger regions.
        /// </summary>
        public int RegionMergeArea;

        /// <summary>
        /// The maximum allowed length for contour edges along the border of the mesh.
        /// </summary>
        public float MaxEdgeLen;

        /// <summary>
        /// The maximum distance a simplfied contour's border edges should deviate from the original raw contour.
        /// </summary>
        public float MaxEdgeError;

        /// <summary>
        /// Sets the sampling distance to use when generating the detail mesh. (For height detail only.)
        /// </summary>
        public float DetailSamplingDistance;

        /// <summary>
        /// The maximum distance the detail mesh surface should deviate from heightfield data. (For height detail only.)
        /// </summary>
        public float MaxDetailSamplingError;

        public static NavMeshBuildSettings Default()
        {
            return new NavMeshBuildSettings
            {
                CellHeight = 0.2f,
                CellSize = 0.3f,
                TileSize = 64,
                MinRegionArea = 2,
                RegionMergeArea = 20,
                MaxEdgeLen = 12.0f,
                MaxEdgeError = 1.3f,
                DetailSamplingDistance = 6.0f,
                MaxDetailSamplingError = 1.0f,
            };
        }

        public float TileCellSize
        {
            get
            {
                return TileSize * CellSize;
            }
        }

        public bool Equals(NavMeshBuildSettings other)
        {
            return CellHeight.Equals(other.CellHeight) && CellSize.Equals(other.CellSize) && TileSize == other.TileSize && MinRegionArea.Equals(other.MinRegionArea) &&
                   RegionMergeArea.Equals(other.RegionMergeArea) && MaxEdgeLen.Equals(other.MaxEdgeLen) && MaxEdgeError.Equals(other.MaxEdgeError) &&
                   DetailSamplingDistance.Equals(other.DetailSamplingDistance) && MaxDetailSamplingError.Equals(other.MaxDetailSamplingError);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                var hashCode = CellHeight.GetHashCode();
                hashCode = (hashCode * 397) ^ CellSize.GetHashCode();
                hashCode = (hashCode * 397) ^ TileSize;
                hashCode = (hashCode * 397) ^ MinRegionArea.GetHashCode();
                hashCode = (hashCode * 397) ^ RegionMergeArea.GetHashCode();
                hashCode = (hashCode * 397) ^ MaxEdgeLen.GetHashCode();
                hashCode = (hashCode * 397) ^ MaxEdgeError.GetHashCode();
                hashCode = (hashCode * 397) ^ DetailSamplingDistance.GetHashCode();
                hashCode = (hashCode * 397) ^ MaxDetailSamplingError.GetHashCode();
                return hashCode;
            }
        }
    }

}
