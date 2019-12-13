

**Structure**

AiNav is 3 separate libraries.

 - C++ project integrating with recastnavigation. (AiNavInterop)
 - C# project that is not dependent on Unity that has the core interop and navmesh building  flow. (AiNavCore)
 - C# ECS based project that integrates it all with Unity.   Includes navmesh building, pathfinding, and crowd management. (AiNav)

**Focus**

AiNav was created primarily to make runtime tile building fast and low impact.  And to be able to use the detour crowd in Unity jobs, keeping it off the main thread.

It is not a drop in replacement for Unity's navmesh system.  Design decisions especially in the build process were very specific.   Unity's behavior is not going to match up all the time.  Unity's system is based on recast also, but it's likely seen a number of modifications.

 The building is optimized for frequent updates of small numbers of tiles at runtime.  It expects you to feed it already optimized geometry that is not much larger then the tile size.  If you feed it geometry far larger then the tile size it's going to perform badly.

The ECS side comes by default with code that relies on a modified version of Unity.Physics.  No functional changes were made but it has to open up access to a few internal data structures.  This part can fairly easily be commented out or removed.   You can drive everything via mesh sources if you want.

**Unsafe**

There is a lot of unsafe code and a decent amount of C++ code.  You were warned.


**Basic Usage:**

The navigation meshes use a surface abstraction.  Each surface in a scene having a unique id.  A basic build from scratch is these three buttons on the surface component in order:

 - DestroySurfaceData
 -  SaveSourcesAndFilters
 -  Build

Tiles are persisted to streaming assets.  On play the surface MB component will trigger creating the surface controller, and the navmesh will be automatically loaded.

**Sources**

AiNav supports Unity.Physics collider sources, or mesh sources.  The former is baked in.  The latter you provide.  You can provide mesh sources via subclassing MeshSourceAuthorBase.  When you save sources and filters on the surface component, it will gather all mesh source authors in the scene.  There is a simple mesh source author implementation included.  

Layers are supported but primarily for the Unity.Physics support.  It convers the layer and masks to CollisionFilter's and then filters physics geometry by that in addition to tile bounds.

Mesh sources can be shared or non shared. SharedMeshDb is where you configure shared meshes.  Shared meshes are saved as blob assets.  So you can create many mesh sources but all sharing a small number of actual meshes.  There is built in support for primitive types.

You can also set a mesh source to not be collected by the surface.  That's there mainly for testing actually but you might find a practical use for it.

The surface controller also has methods to add/remove sources at runtime.  When you add a source it assigns an id, so you can then use that id to remove the source.  You can also set custom data on the mesh source, and there is a lookup by custom data method on the surface controller.


**Filters**

There is also filtering.  Saving the sources and filters will collect all of the BoxFilter's in the scene.  That is the only filter right now, it's the only use case we actually had but you could extend it to most anything.  There is also the built in water and height filtering on the surface component itself.  This avoids having to create meshes just for waterlines for example.  This filter is also only activated for terrains.  If you want it to apply to other geometry you will need to modify the source.  In our case we explicitly did not want it applying to anything else.

**Regions and flags**

This is a recast thing and we only support basic usage.  63 is the default include region.  0 is the exclude region.  The flag should always be 1.  Regions and flags can be used with query filters to handle a lot of custom pathfinding, but we haven't implemented query filters quite yet.  Mainly because we don't yet have a pressing need, so I haven't sorted out the best api for it.  I'm sure we will eventually need it ourselves so it's likely coming. 

**Building**

Building is controlled by calling the MarkDirty method on the surface controller with a bounds. It will rebuild all tiles in the bounds.

The batch size on the surface component basically controls how many tiles the system will try to process per update.

**Editor tools**

The editor tools work via ECS, creating an editor mode world if needed.  The EcsWorld component in the scene can be used to stop the world and destroy the nav system if you want to reset the state for some reason.  The surface component will automatically trigger world creation if needed.

**Crowd**

For the crowd there is a sample system for how to control agents, and a simple MB component that shows how to access agent data from the main thread (mainly there for testing).  Also one of the core crowd jobs that manages pathing isn't using burst.  Burst changed their api to not allow passing value types via PInvoke.  It's a rather simple fix to get it compatible just haven't gotten around to it.


**Queries**

Navmesh queries are inherently linked to surfaces.  That's just how recast works and why Unity's implementation has a concept of allocating a query.  When a surface is destroyed AiNav marks all queries tied to it as invalid.  It's not disposed you still need to do that, but it will prevent usage of the query so as not to try and reference an invalid pointer.

Jobs  running queries should call AddQueryDependency on the surface controller. 

AiNav makes a clear distinction between building tiles and updating the navmesh.  Unity conflates those things but in recast they are distinctly different.  So while tiles are building there is no conflict with querying, the navmesh is still usable.  It's only a few MS of time where we update the navmesh itself where the navmesh and queries must be synchronized.  That is where we potentially use the dependent JobHandle.  


**Data**

AiNav persists it's data using the entities binary serialization.  The data store by default scopes surfaces to scenes.  There is a constant you can change to make the scope always global if you want.  


**AiNativeArray/AiNativeList**

I threw these in because they solved the problem of I want the core usable outside Unity, and I want to iterate on the interop stuff outside of Unity also.  So these are api compatible for the most part, and outside of I think one usage in the core building flow, optional.  Any/all usages are easy to replace with Unity's containers if you so desire.

**What's Missing**

Query filters are the main obvious thing.  That and better support for regions.

Obstacles.   Tile rebuilds are fast enough that for our needs we don't need obstacle support per say.  So no plans for this.  The recast way involves creating special cache tiles that are a different thing then regular tiles and would require a whole separate build flow and more interop. 

