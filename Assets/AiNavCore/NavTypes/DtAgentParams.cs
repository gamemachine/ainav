using System;
using Unity.Entities;

namespace AiNav
{
    [Serializable]
    public struct DtAgentParams : IComponentData
    {
        public float Radius;                       ///< Agent radius. [Limit: >= 0]
        public float Height;                       ///< Agent height. [Limit: > 0]
        public float MaxAcceleration;              ///< Maximum allowed acceleration. [Limit: >= 0]
        public float MaxSpeed;                     ///< Maximum allowed speed. [Limit: >= 0]
        public float CollisionQueryRange;
        public float PathOptimizationRange;        ///< The path visibility optimization range. [Limit: > 0]
        public float SeparationWeight;
        public int AnticipateTurns;
        public int OptimizeVis;
        public int OptimizeTopo;
        public int ObstacleAvoidance;
        public int CrowdSeparation;
        public int ObstacleAvoidanceType;
        public int QueryFilterType;

        public static DtAgentParams Default
        {
            get
            {
                DtAgentParams ap = new DtAgentParams();
                ap.Radius = 0.5f;
                ap.Height = 2f;
                ap.MaxAcceleration = 6f;
                ap.MaxSpeed = 3f;
                ap.CollisionQueryRange = 6f;
                ap.PathOptimizationRange = 15f;
                ap.SeparationWeight = 2f;
                ap.AnticipateTurns = 1;
                ap.OptimizeVis = 1;
                ap.OptimizeTopo = 1;
                ap.ObstacleAvoidance = 1;
                ap.CrowdSeparation = 1;
                ap.ObstacleAvoidanceType = 3;
                ap.QueryFilterType = 0;
                return ap;
            }
        }

        public static DtAgentParams Fast
        {
            get
            {
                DtAgentParams ap = new DtAgentParams();
                ap.Radius = 0.5f;
                ap.Height = 2f;
                ap.MaxAcceleration = 6f;
                ap.MaxSpeed = 3f;
                ap.CollisionQueryRange = 3f;
                ap.PathOptimizationRange = 7.5f;
                ap.SeparationWeight = 0.5f;
                ap.AnticipateTurns = 1;
                ap.OptimizeVis = 0;
                ap.OptimizeTopo = 0;
                ap.ObstacleAvoidance = 1;
                ap.CrowdSeparation = 1;
                ap.ObstacleAvoidanceType = 0;
                ap.QueryFilterType = 0;
                return ap;
            }
        }

        public bool Equals(DtAgentParams other)
        {
            return Radius.Equals(other.Radius) && Height.Equals(other.Height) && MaxAcceleration.Equals(other.MaxAcceleration) && MaxSpeed.Equals(other.MaxSpeed) &&
                CollisionQueryRange.Equals(other.CollisionQueryRange) && PathOptimizationRange.Equals(other.PathOptimizationRange) &&
                SeparationWeight.Equals(other.SeparationWeight) && AnticipateTurns.Equals(other.AnticipateTurns) && OptimizeVis.Equals(other.OptimizeVis) &&
                OptimizeTopo.Equals(other.OptimizeTopo) && ObstacleAvoidance.Equals(other.ObstacleAvoidance) && CrowdSeparation.Equals(other.CrowdSeparation) &&
                ObstacleAvoidanceType.Equals(other.ObstacleAvoidanceType) && SeparationWeight.Equals(other.SeparationWeight);
        }
    }
}
