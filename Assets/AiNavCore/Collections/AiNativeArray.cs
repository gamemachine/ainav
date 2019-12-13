using System;
using Unity.Collections.LowLevel.Unsafe;

namespace AiNav.Collections
{
    public unsafe struct AiNativeArray<T> where T : unmanaged
    {
        public int Length => m_Data->Length;
        [NativeDisableUnsafePtrRestriction]
        private UnsafeArrayData* m_Data;

        public int SizeOf
        {
            get
            {
                return UnsafeCollectionUtility.SizeOf<T>() * Length;
            }
        }

        public T this[int i]
        {
            get
            {
                return m_Data->ReadElement<T>(i);
            }

            set
            {
                m_Data->WriteElement<T>(i, value);
            }
        }

        public AiNativeArray(int length)
        {
            if (length <= 0)
            {
                throw new ArgumentException("Length must be > 0");
            }

            m_Data = UnsafeArrayData.Create(UnsafeCollectionUtility.SizeOf<T>(), length);
        }

        public void* GetUnsafePtr()
        {
            return m_Data->GetUnsafePtr();
        }

        public void Dispose()
        {
            UnsafeArrayData.Destroy(m_Data);
            m_Data = null;
        }

        public AiNativeList<T> ToNativeList()
        {
            var data = UnsafeListData.CreateFrom(this);
            AiNativeList<T> list = new AiNativeList<T>(data);
            Dispose();
            return list;
        }

    }
}
