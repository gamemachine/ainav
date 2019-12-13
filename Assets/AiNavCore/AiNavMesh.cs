using System;
using System.Collections.Generic;
using Unity.Mathematics;

namespace AiNav
{
    public class AiNavMesh : IDisposable
    {
        private static int NextId = 1;

        public int Id;
        public float TileSize;
        public float CellSize;

        private HashSet<int2> TileCoordinates = new HashSet<int2>();

        public IntPtr DtNavMesh { get; private set; }

        public AiNavMesh(float tileSize, float cellSize)
        {
            DtNavMesh = Navigation.NavMesh.CreateNavmesh(tileSize * cellSize);

            Id = NextId;
            NextId++;

            AiNavWorld.Instance.RegisterNavMesh(Id);
        }

        public void Dispose()
        {
            AiNavWorld.Instance.UnregisterNavMesh(Id);

            if (DtNavMesh != IntPtr.Zero)
            {
                Navigation.NavMesh.DestroyNavmesh(DtNavMesh);
                DtNavMesh = IntPtr.Zero;
            }
        }

        public void AddOrReplaceTiles(List<byte[]> tiles)
        {
            foreach (byte[] tileData in tiles)
            {
                AddOrReplaceTile(tileData);
            }
        }

        /// <summary>
        /// Adds or replaces a tile in the navigation mesh
        /// </summary>
        /// <remarks>The coordinate of the tile is embedded inside the tile data header</remarks>
        public unsafe bool AddOrReplaceTile(byte[] data)
        {
            fixed (byte* dataPtr = data)
            {
                DtTileHeader* header = (DtTileHeader*)dataPtr;
                var coord = new int2(header->X, header->Y);

                // Remove old tile if it exists
                RemoveTile(coord);

                TileCoordinates.Add(coord);
                return Navigation.NavMesh.AddTile(DtNavMesh, new IntPtr(dataPtr), data.Length) == 1;
            }
        }

        /// <summary>
        /// Removes a tile at given coordinate
        /// </summary>
        /// <param name="coord">The tile coordinate</param>
        public bool RemoveTile(int2 coord)
        {
            if (!TileCoordinates.Contains(coord))
                return false;

            TileCoordinates.Remove(coord);
            return Navigation.NavMesh.RemoveTile(DtNavMesh, coord) == 1;
        }

       
    }
}
