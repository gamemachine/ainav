using System.Globalization;

namespace AiNav
{
    public struct NavMeshBuildResult
    {
        public int Result;
        public int TilesBuilt;
        public int VerticeCount;
        public int TriangleCount;

        public bool IsValidBuild
        {
            get
            {
                return (Result == 0 || Result == 110);
            }
        }

        public override string ToString()
        {
            return string.Format(CultureInfo.CurrentCulture, "Result:{0} TilesBuilt:{1} VerticeCount:{2} TriangleCount:{3}", Result, TilesBuilt, VerticeCount, TriangleCount);
        }
    }

}
