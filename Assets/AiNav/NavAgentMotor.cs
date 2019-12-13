using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

namespace AiNav
{
    public class NavAgentMotor : MonoBehaviour
    {
        [SerializeField]
        public int SurfaceId;
        [SerializeField]
        private int CrowdIndex;
        [SerializeField]
        private NavAgentDebug NavAgentDebug;
        

        private SurfaceController Controller;

        private void Start()
        {

            var navSystem = EcsWorld.Active.GetExistingSystem<AiNavSystem>();
            Controller = navSystem.GetSurfaceController(SurfaceId);
            if (Controller == null)
            {
                Destroy(gameObject);
                return;
            }

            var query = new AiNavQuery(Controller.NavMesh, 2048);
            float3 extent = new float3(5f, 5f, 5f);
            float3 position = transform.position;
            if (!query.GetRandomPosition(ref position))
            {
                query.Dispose();
                Debug.Log("Spawn position not found");
                Destroy(gameObject);
                return;
            }
            query.Dispose();


            DtAgentParams agentParams = DtAgentParams.Default;
            agentParams.MaxSpeed = 6f;

            if (Controller.CrowdController.TryAddAgent(position, agentParams, out NavAgent agent))
            {
                var world = EcsWorld.Active;
                Entity entity = world.EntityManager.CreateEntity();
                world.EntityManager.AddComponentData(entity, agent);
                world.EntityManager.AddComponentData(entity, new AgentPathData());
                world.EntityManager.AddBuffer<AgentPathBuffer>(entity);
                transform.position = position;
                CrowdIndex = agent.CrowdIndex;
            } else
            {
                Debug.Log("Failed adding agent");
                Destroy(gameObject);
                return;
            }
            
        }

        private void Update()
        {
            if (!Controller.CrowdController.TryGetAgent(CrowdIndex, out NavAgentDebug))
            {
                return;
            }

            NavAgent agent = NavAgentDebug.NavAgent;
            AgentPathData pathData = NavAgentDebug.PathData;
           
            //transform.position = Vector3.MoveTowards(transform.position, readOnlyAgent.DtCrowdAgent.Position, Time.deltaTime * 10f);
            transform.position = agent.DtCrowdAgent.Position;

        }
    }
}
