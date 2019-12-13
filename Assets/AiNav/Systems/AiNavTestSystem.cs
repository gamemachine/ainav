using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;

namespace AiNav
{
    public class AiNavTestSystem : JobComponentSystem
    {
        public const int SurfaceId = 1;
        private SurfaceController Controller;
        private AiNavQuery Query;
        private NativeArray<Unity.Mathematics.Random> Random;

        private void OnSurfaceCreated(int surfaceId)
        {
            if (surfaceId != SurfaceId) return;
            var navSystem = EcsWorld.Active.GetExistingSystem<AiNavSystem>();
            Controller = navSystem.GetSurfaceController(SurfaceId);

            Query = new AiNavQuery(Controller.NavMesh, 2048);
            Debug.Assert(Query.IsValid());
        }

        private void OnSurfaceDestroyed(int surfaceId)
        {
            if (surfaceId != SurfaceId) return;
            Query.Dispose();
            Controller = null;
        }

        protected override void OnCreate()
        {
            base.OnCreate();
            var navSystem = EcsWorld.Active.GetExistingSystem<AiNavSystem>();
            AiNavSystem.OnSurfaceCreated += OnSurfaceCreated;
            AiNavSystem.OnSurfaceDestroyed += OnSurfaceDestroyed;
            Random = new NativeArray<Unity.Mathematics.Random>(1, Allocator.Persistent);
            Random[0] = new Unity.Mathematics.Random((uint)UnityEngine.Random.Range(int.MinValue, int.MaxValue));
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();
            AiNavSystem.OnSurfaceCreated -= OnSurfaceCreated;
            AiNavSystem.OnSurfaceDestroyed -= OnSurfaceDestroyed;
            if (Random.IsCreated) Random.Dispose();
        }

        protected override JobHandle OnUpdate(JobHandle inputDeps)
        {
            if (Controller == null)
            {
                return inputDeps;
            }
            
            MoveAgentsJob moveJob = new MoveAgentsJob
            {
                DeltaTime = Time.deltaTime,
                Query = Query,
                Random = Random
            };
            inputDeps = moveJob.ScheduleSingle(this, inputDeps);

            Controller.AddQueryDependency(inputDeps);

            return inputDeps;
        }

        [BurstCompile]
        unsafe struct MoveAgentsJob : IJobForEach<NavAgent>
        {
            public float DeltaTime;
            public AiNavQuery Query;
            public NativeArray<Unity.Mathematics.Random> Random;

            public void Execute(ref NavAgent agent)
            {
                float distanceToDest = math.distance(agent.Destination, agent.DtCrowdAgent.Position);

                // get next random dest
                if (distanceToDest <= 0.5f || agent.SetDestinationFailed)
                {
                    var random = Random[0];
                    if (agent.ClosestRandomPointOnCircle(Query, ref random, 40f, out float3 closest))
                    {
                        agent.Destination = closest;
                        agent.SetDestination = true;
                    }
                    Random[0] = random;
                }

            }
        }

        
    }
}
