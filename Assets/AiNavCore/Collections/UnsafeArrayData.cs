using System;
using System.Runtime.InteropServices;

namespace AiNav.Collections
{
    public unsafe struct UnsafeArrayData
    {
        public void* Ptr;
        public int Length;

        public static UnsafeArrayData* Create(int sizeOf, int length)
        {
            UnsafeArrayData* arrayData = (UnsafeArrayData*)Marshal.AllocHGlobal(UnsafeCollectionUtility.SizeOf<UnsafeArrayData>());

            var bytesToMalloc = sizeOf * length;
            arrayData->Ptr = (void*)Marshal.AllocHGlobal(bytesToMalloc);
            arrayData->Length = length;

            return arrayData;
        }

        public static void Destroy(UnsafeArrayData* listData)
        {
#if DEBUG || ENABLE_UNITY_COLLECTIONS_CHECKS
            if (listData == null)
            {
                throw new InvalidOperationException("UnsafeListData has yet to be created or has been destroyed!");
            }
#endif
            listData->Dispose();
            Marshal.FreeHGlobal((IntPtr)listData);
            listData = null;
        }

        public void Dispose()
        {
            Marshal.FreeHGlobal((IntPtr)Ptr);
            Ptr = null;
            Length = 0;
        }

        public T ReadElement<T>(int index) where T : unmanaged
        {
#if DEBUG || ENABLE_UNITY_COLLECTIONS_CHECKS
            RequireCreated();
            RequireIndexInBounds(index);
#endif
            return UnsafeCollectionUtility.ReadArrayElement<T>(Ptr, index);
        }

        public void WriteElement<T>(int index, T value) where T : unmanaged
        {
#if DEBUG || ENABLE_UNITY_COLLECTIONS_CHECKS
            RequireCreated();
            RequireIndexInBounds(index);
#endif
            UnsafeCollectionUtility.WriteArrayElement<T>(Ptr, index, value);
        }

        public void* GetUnsafePtr()
        {
#if DEBUG || ENABLE_UNITY_COLLECTIONS_CHECKS
            RequireCreated();
#endif
            return Ptr;
        }

        public void RequireCreated()
        {
            if (Ptr == null)
            {
                throw new System.InvalidOperationException("UnsafeArray not created");
            }
        }

        private void RequireIndexInBounds(int index)
        {
            if (index < 0 || index >= Length)
            {
                throw new System.InvalidOperationException(string.Format("Index {0} out of bounds of length {1}: ", index, Length));
            }
        }
    }
}
