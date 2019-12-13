using AiNav.Collections;
using System;
using System.Collections.Generic;
using Unity.Mathematics;

namespace AiNav
{
    public class NavMeshTile : IEquatable<NavMeshTile>
    {
        public byte[] Data;

        public unsafe int2 Coord
        {
            get
            {
                fixed (byte* dataPtr = Data)
                {
                    DtTileHeader* header = (DtTileHeader*)dataPtr;
                    int2 coord = new int2(header->X, header->Y);
                    return coord;
                }
            }
        }

        public unsafe void AppendStats(Dictionary<byte, int> stats)
        {
            if (Data == null || Data.Length == 0)
                return;

            fixed (byte* dataPtr = Data)
            {
                DtTileHeader* header = (DtTileHeader*)dataPtr;
                if (header->VertCount == 0)
                    return;

                int headerSize = Navigation.DtAlign4(sizeof(DtTileHeader));
                int vertsSize = Navigation.DtAlign4(sizeof(float) * 3 * header->VertCount);

                byte* ptr = dataPtr;
                ptr += headerSize;

                float3* vertexPtr = (float3*)ptr;
                ptr += vertsSize;
                DtPoly* polyPtr = (DtPoly*)ptr;

                for (int i = 0; i < header->PolyCount; i++)
                {
                    // Expand polygons into triangles
                    DtPoly poly = polyPtr[i];

                    if (!stats.TryGetValue(poly.AreaAndType, out int count))
                    {
                        stats[poly.AreaAndType] = 0;
                    }
                    stats[poly.AreaAndType]++;

                }
            }
        }


        public unsafe bool GetTileVertices(AiNativeList<float3> vertices, AiNativeList<int> indices)
        {
            if (Data == null || Data.Length == 0)
                return false;

            fixed (byte* dataPtr = Data)
            {
                DtTileHeader* header = (DtTileHeader*)dataPtr;
                if (header->VertCount == 0)
                    return false;

                int headerSize = Navigation.DtAlign4(sizeof(DtTileHeader));
                int vertsSize = Navigation.DtAlign4(sizeof(float) * 3 * header->VertCount);

                byte* ptr = dataPtr;
                ptr += headerSize;

                float3* vertexPtr = (float3*)ptr;
                ptr += vertsSize;
                DtPoly* polyPtr = (DtPoly*)ptr;

                for (int i = 0; i < header->VertCount; i++)
                {
                    vertices.Add(vertexPtr[i]);
                }

                for (int i = 0; i < header->PolyCount; i++)
                {
                    // Expand polygons into triangles
                    var poly = polyPtr[i];
                    for (int j = 0; j <= poly.VertexCount - 3; j++)
                    {
                        indices.Add(poly.Vertices[0]);
                        indices.Add(poly.Vertices[j + 1]);
                        indices.Add(poly.Vertices[j + 2]);
                    }
                }

                return true;
            }
        }

        public bool Equals(NavMeshTile other)
        {
            int2 coord = Coord;
            int2 otherCoord = other.Coord;
            return coord.Equals(otherCoord);
        }

        public override int GetHashCode()
        {
            return (int)math.hash(Coord);
        }

    }
}
