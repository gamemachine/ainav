#pragma once
#include <cstdint>

//#pragma pack(push, 4)

struct int2
{
	int x;
	int y;
};

typedef struct float3
{
	float x;
	float y;
	float z;
} float3;

typedef struct DtBoundingBox
{
	float3 min;
	float3 max;
} DtBoundingBox;

struct DtBuildSettings
{
	// Bounding box for the generated navigation mesh
	DtBoundingBox boundingBox;
	float cellHeight;
	float cellSize;
	int tileSize;
	int2 tilePosition;
	int regionMinArea;
	int regionMergeArea;
	float edgeMaxLen;
	float edgeMaxError;
	float detailSampleDistInput;
	float detailSampleMaxErrorInput;
	float agentHeight;
	float agentRadius;
	float agentMaxClimb;
	float agentMaxSlope;
};

struct DtGeneratedData
{
	bool success;
	int error = 0;
	float3* navmeshVertices = nullptr;
	int numNavmeshVertices = 0;
	uint8_t* navmeshData = nullptr;
	int navmeshDataLength = 0;
};


struct DtCrowdAgent
{
	int active;
	int partial;
	float desiredSpeed;
	float3 position;
	float3 velocity;
};

struct DtCrowdAgentsResult
{
	DtCrowdAgent* agents = nullptr;
	int agentCount = 0;
};

struct DtAgentParams {
	float radius;						///< Agent radius. [Limit: >= 0]
	float height;						///< Agent height. [Limit: > 0]
	float maxAcceleration;				///< Maximum allowed acceleration. [Limit: >= 0]
	float maxSpeed;						///< Maximum allowed speed. [Limit: >= 0]
	float collisionQueryRange;
	float pathOptimizationRange;		///< The path visibility optimization range. [Limit: > 0]
	float separationWeight;
	int anticipateTurns;
	int optimizeVis;
	int optimizeTopo;
	int obstacleAvoidance;
	int crowdSeparation;
	int obstacleAvoidanceType;
	int queryFilterType;
};

//#pragma pack(pop)
