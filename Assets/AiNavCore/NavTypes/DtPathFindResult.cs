using System;

namespace AiNav
{
    [Serializable]
    public struct DtPathFindResult
    {
        public bool PathFound;

        /// <summary>
        /// Should point to a preallocated array of <see cref="Vector3"/>'s matching the amount in <see cref="DtPathFindQuery.MaxPathPoints"/>
        /// </summary>
        public IntPtr PathPoints;

        public int NumPathPoints;
    }
}
