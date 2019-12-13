using System.Runtime.InteropServices;
using Unity.Collections.LowLevel.Unsafe;

namespace AiNav.Collections
{
    [StructLayout(LayoutKind.Sequential)]
    public unsafe struct AiNativeList<T> where T : unmanaged
    {
        public int Capacity => m_ListData->Capacity;
        public int Length => m_ListData->Length;
        [NativeDisableUnsafePtrRestriction]
        private UnsafeListData* m_ListData;

        public T this[int i]
        {
            get
            {
                return m_ListData->ReadElement<T>(i);
            }

            set
            {
                m_ListData->WriteElement<T>(i, value);
            }
        }

        public AiNativeList(UnsafeListData* m_ListData)
        {
            this.m_ListData = m_ListData;
        }

        public AiNativeList(int initialCapacity)
        {
            m_ListData = UnsafeListData.Create(UnsafeCollectionUtility.SizeOf<T>(), initialCapacity);
        }

        public void Clear()
        {
            m_ListData->Clear();
        }

        public void Add(T value)
        {
            m_ListData->Add(value);
        }

        public void AddRange(AiNativeList<T> list)
        {
            m_ListData->AddRange<T>(list.m_ListData->Ptr, list.Length);
        }

        public void RemoveAtSwapBack(int index)
        {
            m_ListData->RemoveAtSwapBack<T>(index);
        }

        public void* GetUnsafePtr()
        {
            return m_ListData->GetUnsafePtr();
        }

        public void Dispose()
        {
            UnsafeListData.Destroy(m_ListData);
        }

    }
}
