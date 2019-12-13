using Unity.Collections;
using UnityEngine;

namespace AiNav
{
    public class MeshSourceAuthorBase : MonoBehaviour
    {
        public virtual void AppendSources(int surfaceId, NativeList<MeshSource> nonShared, NativeList<MeshSource> shared)
        {

        }
    }
}
