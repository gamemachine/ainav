using System;

namespace AiNav
{
    [Serializable]
    public struct NavAgentSettings
    {
        /// <summary>
        /// Height of the actor
        /// </summary>
        /// <userdoc>
        /// The height of the entities in this group. Entities can't enter areas with ceilings lower than this value.
        /// </userdoc>
        public float Height;

        /// <summary>
        /// Maximum vertical distance this agent can climb
        /// </summary>
        /// <userdoc>
        /// The maximum height that entities in this group can climb. 
        /// </userdoc>
        public float MaxClimb;

        /// <summary>
        /// Maximum slope angle this agent can climb
        /// </summary>
        /// <userdoc>
        /// The maximum incline (in degrees) that entities in this group can climb. Entities can't go up or down slopes higher than this value. 
        /// </userdoc>
        public float MaxSlope;

        /// <summary>
        /// Radius of the actor
        /// </summary>
        /// <userdoc>
        /// The larger this value, the larger the area of the navigation mesh entities use. Entities can't pass through gaps of less than twice the radius.
        /// </userdoc>
        public float Radius;

        public static NavAgentSettings Default()
        {
            return new NavAgentSettings
            {
                Height = 2.0f,
                MaxClimb = 0.4f,
                MaxSlope = 45f,
                Radius = 0.5f,
            };
        }

    }
}
