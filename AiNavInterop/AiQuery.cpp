#include "NavigationMesh.hpp"
#include "AiQuery.hpp"
#include <DetourCommon.h>

static float frand()
{
	return (float)rand() / (float)RAND_MAX;
}

AiQuery::AiQuery()
{
}

AiQuery::~AiQuery()
{
	if (m_navQuery)
		dtFreeNavMeshQuery(m_navQuery);
}

int AiQuery::Init(NavigationMesh* navmesh, int maxNodes)
{
	m_navMesh = navmesh->GetNavmesh();
	m_navQuery = dtAllocNavMeshQuery();

	dtStatus status = m_navQuery->init(m_navMesh, maxNodes);
	if (dtStatusFailed(status))
		return 0;
	return 1;
}

void AiQuery::Invalidate()
{
	invalidated = 1;
}

int AiQuery::IsValid()
{
	return invalidated == 0;
}

int AiQuery::GetRandomPosition(float3* result)
{
	if (invalidated == 1)
		return 0;

	dtPolyRef startPoly;
	float3 startPoint;
	dtQueryFilter filter;
	dtStatus status;

	status = m_navQuery->findRandomPoint(&filter, frand, &startPoly, &startPoint.x);
	if (dtStatusFailed(status)) {
		return 0;
	}
	result->x = startPoint.x;
	result->y = startPoint.y;
	result->z = startPoint.z;
	return 1;
}

int AiQuery::SamplePosition(float3 point, float3 extent, float3* result)
{
	if (invalidated == 1)
		return 0;

	dtPolyRef startPoly;
	float3 startPoint;

	dtQueryFilter filter;
	dtStatus status;

	status = m_navQuery->findNearestPoly(&point.x, &extent.x, &filter, &startPoly, &startPoint.x);
	if (dtStatusFailed(status) || !startPoly) {
		return 0;
	}

	result->x = startPoint.x;
	result->y = startPoint.y;
	result->z = startPoint.z;
	return 1;
}

int AiQuery::GetLocation(float3 point, float3 extent, float3* result) {

	if (invalidated == 1)
		return 0;

	dtPolyRef startPoly;
	float3 startPoint;

	dtQueryFilter filter;
	dtStatus status;

	float npos[3];
	bool overlay = false;

	status = m_navQuery->findNearestPoly(&point.x, &extent.x, &filter, &startPoly, npos);
	if (dtStatusFailed(status) || !startPoly) {
		return 0;
	}

	status = m_navQuery->closestPointOnPoly(startPoly, npos, &startPoint.x, &overlay);
	if (dtStatusFailed(status)) {
		return 0;
	}

	result->x = startPoint.x;
	result->y = startPoint.y;
	result->z = startPoint.z;
	return 1;

}

int AiQuery::HasPath(NavMeshPathfindQuery query)
{
	if (invalidated == 1)
		return 0;

	dtPolyRef startPoly, endPoly;
	float3 startPoint, endPoint;

	// Find the starting polygons and point on it to start from
	dtQueryFilter filter;
	dtStatus status;
	status = m_navQuery->findNearestPoly(&query.source.x, &query.findNearestPolyExtent.x, &filter, &startPoly, &startPoint.x);
	if (dtStatusFailed(status))
		return 0;
	status = m_navQuery->findNearestPoly(&query.target.x, &query.findNearestPolyExtent.x, &filter, &endPoly, &endPoint.x);
	if (dtStatusFailed(status))
		return 0;

	std::vector<dtPolyRef> polys;
	polys.resize(query.maxPathPoints);
	int pathPointCount = 0;
	status = m_navQuery->findPath(startPoly, endPoly, &startPoint.x, &endPoint.x,
		&filter, polys.data(), &pathPointCount, polys.size());
	if (dtStatusFailed(status) || (status & DT_PARTIAL_RESULT) != 0)
		return 0;

	return 1;
}

void AiQuery::FindStraightPath(NavMeshPathfindQuery query, NavMeshPathfindResult* result)
{
	if (invalidated == 1)
		return;

	// Reset result
	result->pathFound = false;
	dtPolyRef startPoly, endPoly;
	float3 startPoint, endPoint;

	// Find the starting polygons and point on it to start from
	dtQueryFilter filter;
	dtStatus status;
	status = m_navQuery->findNearestPoly(&query.source.x, &query.findNearestPolyExtent.x, &filter, &startPoly, &startPoint.x);
	if (dtStatusFailed(status))
		return;
	status = m_navQuery->findNearestPoly(&query.target.x, &query.findNearestPolyExtent.x, &filter, &endPoly, &endPoint.x);
	if (dtStatusFailed(status))
		return;

	std::vector<dtPolyRef> polys;
	polys.resize(query.maxPathPoints);
	int pathPointCount = 0;
	status = m_navQuery->findPath(startPoly, endPoly, &startPoint.x, &endPoint.x,
		&filter, polys.data(), &pathPointCount, polys.size());
	if (dtStatusFailed(status) || (status & DT_PARTIAL_RESULT) != 0)
		return;

	std::vector<float3> straightPath;
	std::vector<uint8_t> straightPathFlags;
	std::vector<dtPolyRef> straightpathPolys;
	straightPath.resize(query.maxPathPoints);
	straightPathFlags.resize(query.maxPathPoints);
	straightpathPolys.resize(query.maxPathPoints);
	status = m_navQuery->findStraightPath(&startPoint.x, &endPoint.x,
		polys.data(), pathPointCount,
		(float*)result->pathPoints, straightPathFlags.data(), straightpathPolys.data(),
		&result->numPathPoints, query.maxPathPoints);
	if (dtStatusFailed(status))
		return;
	result->pathFound = true;
}

void AiQuery::Raycast(NavMeshRaycastQuery query, NavMeshRaycastResult* result)
{
	if (invalidated == 1)
		return;

	// Reset result
	result->hit = false;
	dtQueryFilter filter;

	dtPolyRef startPoly;
	dtStatus status = m_navQuery->findNearestPoly(&query.start.x, &query.findNearestPolyExtent.x, &filter, &startPoly, 0);
	if (dtStatusFailed(status))
		return;

	float t;
	std::vector<dtPolyRef> polys;
	polys.resize(query.maxPathPoints);
	int raycastPolyCount = 0;
	status = m_navQuery->raycast(startPoly, &query.start.x, &query.end.x, &filter, &t, &result->normal.x, polys.data(), &raycastPolyCount, polys.size());
	if (dtStatusFailed(status))
		return;

	result->hit = true;
	dtVlerp(&result->position.x, &query.start.x, &query.end.x, t);
}
