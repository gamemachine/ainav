#pragma once

#include "Navigation.hpp"
#include "NavigationMesh.hpp"
#include <corecrt_memory.h>
#include <DetourCommon.h>
#include <memory>

static float frand()
{
	return (float)rand() / (float)RAND_MAX;
}


NavigationMesh::NavigationMesh()
{
}

NavigationMesh::~NavigationMesh()
{
	// Cleanup allocated tiles
	for (auto tile : m_tileRefs)
	{
		uint8_t* deletedData;
		int deletedDataLength = 0;
		dtStatus status = m_navMesh->removeTile(tile, &deletedData, &deletedDataLength);
		if (dtStatusSucceed(status))
		{
			if (deletedData)
				delete[] deletedData;
		}
	}

	if (m_navQuery) {
		dtFreeNavMeshQuery(m_navQuery);
		m_navQuery = nullptr;
	}
		

	if (m_navMesh) {
		dtFreeNavMesh(m_navMesh);
		m_navMesh = nullptr;
	}
		
}

int NavigationMesh::Init(float cellTileSize)
{
	// Allocate objects
	m_navMesh = dtAllocNavMesh();
	m_navQuery = dtAllocNavMeshQuery();

	if (!m_navMesh || !m_navQuery)
		return 0;

	dtNavMeshParams params = { 0 };
	params.orig[0] = 0.0f;
	params.orig[1] = 0.0f;
	params.orig[2] = 0.0f;
	params.tileWidth = cellTileSize;
	params.tileHeight = cellTileSize;

	// TODO: Link these parameters to the builder
	int tileBits = 14;
	if (tileBits > 14) tileBits = 14;
	int polyBits = 22 - tileBits;
	params.maxTiles = 1 << tileBits;
	params.maxPolys = 1 << polyBits;

	dtStatus status = m_navMesh->init(&params);
	if (dtStatusFailed(status))
		return 0;

	// Initialize the query object
	status = m_navQuery->init(m_navMesh, 2048);
	if (dtStatusFailed(status))
		return 0;
	return 1;
}

int NavigationMesh::LoadTile(uint8_t* navData, int navDataLength)
{
	if (!m_navMesh || !m_navQuery)
		return 0;
	if (!navData)
		return 0;

	// Copy data
	uint8_t* dataCopy = new uint8_t[navDataLength];
	memcpy(dataCopy, navData, navDataLength);

	dtTileRef tileRef = 0;
	if (dtStatusSucceed(m_navMesh->addTile(dataCopy, navDataLength, 0, 0, &tileRef)))
	{
		m_tileRefs.insert(tileRef);
		return 1;
	}

	delete[] dataCopy;
	return 0;
}

int NavigationMesh::RemoveTile(int2 tileCoordinate)
{
	dtTileRef tileRef = m_navMesh->getTileRefAt(tileCoordinate.x, tileCoordinate.y, 0);

	uint8_t* deletedData;
	int deletedDataLength = 0;
	dtStatus status = m_navMesh->removeTile(tileRef, &deletedData, &deletedDataLength);
	if (dtStatusSucceed(status))
	{
		if (deletedData)
			delete[] deletedData;
		m_tileRefs.erase(tileRef);
		return 1;
	}

	return 0;
}

int NavigationMesh::GetRandomPosition(float3* result)
{
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

int NavigationMesh::SamplePosition(float3 point, float3 extent, float3* result)
{
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

int NavigationMesh::GetLocation(float3 point, float3 extent, float3* result) {

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

dtNavMeshQuery* NavigationMesh::GetNavmeshQuery()
{
	return m_navQuery;
}

dtNavMesh* NavigationMesh::GetNavmesh()
{
	return m_navMesh;
}

void NavigationMesh::FindPath(NavMeshPathfindQuery query, NavMeshPathfindResult* result)
{
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

void NavigationMesh::Raycast(NavMeshRaycastQuery query, NavMeshRaycastResult* result)
{
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
