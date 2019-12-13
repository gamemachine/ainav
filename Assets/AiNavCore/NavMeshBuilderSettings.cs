using System;

namespace AiNav
{
    [Serializable]
    public struct NavMeshBuilderSettings
    {
        public int Id;
        public NavMeshBuildSettings BuildSettings;
        public NavAgentSettings AgentSettings;

        public NavMeshBuilderSettings(int id, NavMeshBuildSettings buildSettings, NavAgentSettings agentSettings)
        {
            Id = id;
            BuildSettings = buildSettings;
            AgentSettings = agentSettings;
        }
    }
}
