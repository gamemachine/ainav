#pragma once
#include <DetourNavMesh.h>
#include <DetourNavMeshQuery.h>
#include "NavigationMesh.hpp"

class AiQuery {
private:
	dtNavMesh* m_navMesh = nullptr;
	dtNavMeshQuery* m_navQuery = nullptr;
	int invalidated = 0;
public:
	AiQuery();
	~AiQuery();
	int Init(NavigationMesh* navmesh, int maxNodes);
	void FindStraightPath(NavMeshPathfindQuery query, NavMeshPathfindResult* result);
	int HasPath(NavMeshPathfindQuery query);
	void Raycast(NavMeshRaycastQuery query, NavMeshRaycastResult* result);
	int SamplePosition(float3 point, float3 extent, float3* result);
	int GetRandomPosition(float3* result);
	int GetLocation(float3 point, float3 extent, float3* result);
	int IsValid();
	void Invalidate();
};