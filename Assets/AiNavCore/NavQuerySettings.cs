using System;
using Unity.Mathematics;

namespace AiNav
{
    [Serializable]
    public struct NavQuerySettings
    {
        /// <summary>
        /// The default settings that are used when querying navigation meshes
        /// </summary>
        public static readonly NavQuerySettings Default = new NavQuerySettings
        {
            FindNearestPolyExtent = new float3(2.0f, 4.0f, 2.0f),
            MaxPathPoints = 512,
        };

        /// <summary>
        /// Used as the extend for the find nearest poly bounding box used when scanning for a polygon corresponding to the given starting/ending position. 
        /// Making this bigger will allow you to find paths that allow the entity to start further away or higher from the navigation mesh bounds for example
        /// </summary>
        public float3 FindNearestPolyExtent;

        /// <summary>
        /// The maximum number of path points used internally and also the maximum number of output points
        /// </summary>
        public int MaxPathPoints;
    }
}
