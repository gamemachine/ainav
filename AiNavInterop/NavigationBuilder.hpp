#pragma once
#include "Recast.h"
#include "Navigation.hpp"
#include <cstdint>

class NavigationBuilder
{
	rcHeightfield* m_solid = nullptr;
	uint8_t* m_triareas = nullptr;
	rcCompactHeightfield* m_chf = nullptr;
	rcContourSet* m_cset = nullptr;
	rcPolyMesh* m_pmesh = nullptr;
	rcPolyMeshDetail* m_dmesh = nullptr;
	DtBuildSettings m_buildSettings;
	rcContext* m_context;

	// Detour returned navigation mesh data
	// free with dtFree()
	uint8_t* m_navmeshData = nullptr;
	int m_navmeshDataLength = 0;

	DtGeneratedData m_result;
public:
	NavigationBuilder();
	~NavigationBuilder();
	void Cleanup();
	DtGeneratedData* BuildNavmesh(float3* vertices, int numVertices, int* indices, int numIndices, uint8_t* areas);
	void SetSettings(DtBuildSettings buildSettings);

private:
	int CreateDetourMesh();
};