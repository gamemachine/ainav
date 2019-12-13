using System;
using System.Collections.Generic;
using System.Linq;

namespace AiNav
{
    public class AiNavWorld
    {
        static readonly AiNavWorld instance = new AiNavWorld();

        public static AiNavWorld Instance
        {
            get
            {
                return instance;
            }
        }

        private HashSet<int> RegisteredNavigationMeshes = new HashSet<int>();
        private Dictionary<int, AiNavQuery> Queries = new Dictionary<int, AiNavQuery>();

        private AiNavWorld()
        {
        }

        public void Dispose()
        {
            RegisteredNavigationMeshes.Clear();
            Queries.Clear();
        }

        public void RegisterNavMesh(int id)
        {
            RegisteredNavigationMeshes.Add(id);
        }

        // Queries still linked to a navmesh that is being disposed are marked invalid on the native side.
        //  Disposing them here would be bad, we just need to make sure on the native side it's not trying to use the navmesh which at this point is an invalid pointer.
        // This will make all calls using the invalidated navquery fail from here on out.
        public void UnregisterNavMesh(int id)
        {
            if (!RegisteredNavigationMeshes.Contains(id))
            {
                throw new InvalidOperationException(string.Format("AiNavMesh not found {0}", id));
            }

            var keys = Queries.Keys.ToList();
            foreach(int key in Queries.Keys.ToList())
            {
                AiNavQuery query = Queries[key];
                if (query.NavMeshId == id)
                {
                    query.Invalidate();
                    Queries.Remove(key);
                }
            }
            RegisteredNavigationMeshes.Remove(id);
        }

        public void RegisterQuery(AiNavQuery query)
        {
            if (query.Id <= 0)
            {
                throw new InvalidOperationException(string.Format("AiQuery invalid id {0}", query.Id));
            }

            Queries[query.Id] = query;
        }

        public void UnregisterQuery(int id)
        {
            Queries.Remove(id);
        }
    }
}
