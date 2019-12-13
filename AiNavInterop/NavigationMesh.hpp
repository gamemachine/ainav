#pragma once
#include <DetourNavMeshQuery.h>
#include <cstdint>
#include "Navigation.hpp"
#include <unordered_set>

using namespace std;

//#pragma pack(push, 4)
struct NavMeshPathfindQuery
{
	float3 source;
	float3 target;
	float3 findNearestPolyExtent;
	int maxPathPoints;
};
struct NavMeshPathfindResult
{
	bool pathFound = false;
	float3* pathPoints = nullptr;
	int numPathPoints = 0;
};

struct NavMeshRaycastQuery
{
	float3 start;
	float3 end;
	float3 findNearestPolyExtent;
	int maxPathPoints;
};
struct NavMeshRaycastResult
{
	bool hit = false;
	float3 position;
	float3 normal;
};
//#pragma pack(pop)

class NavigationMesh
{
private:
	dtNavMesh* m_navMesh = nullptr;
	dtNavMeshQuery* m_navQuery = nullptr;
	std::unordered_set<dtTileRef> m_tileRefs;
public:
	
	NavigationMesh();
	~NavigationMesh();
	int Init(float cellTileSize);
	int LoadTile(uint8_t* navData, int navDataLength);
	int RemoveTile(int2 tileCoordinate);
	void FindPath(NavMeshPathfindQuery query, NavMeshPathfindResult* result);
	void Raycast(NavMeshRaycastQuery query, NavMeshRaycastResult* result);
	int SamplePosition(float3 point, float3 extent, float3* result);
	int GetRandomPosition(float3* result);
	dtNavMesh* GetNavmesh();
	dtNavMeshQuery* GetNavmeshQuery();
	int GetLocation(float3 point, float3 extent, float3* result);
};
