using System;
using System.Runtime.InteropServices;

namespace AiNav.Collections
{
    public unsafe struct UnsafeListData
    {
        public void* Ptr;
        public int Length;
        public int Capacity;

        public static UnsafeListData* CreateFrom<T>(byte[] array) where T : unmanaged
        {
            if (array.Length % UnsafeCollectionUtility.SizeOf<T>() != 0)
            {
                throw new ArgumentException("Invalid length for type");
            }

            int size = array.Length / UnsafeCollectionUtility.SizeOf<T>();

            UnsafeListData* listData = Create(UnsafeCollectionUtility.SizeOf<T>(), size);

            listData->Length = size;

            MemoryCopy.Copy(array, 0, listData->Ptr, 0, array.Length);

            return listData;
        }

        public static UnsafeListData* CreateFrom<T>(AiNativeArray<T> array) where T : unmanaged
        {
            UnsafeListData* listData = Create(UnsafeCollectionUtility.SizeOf<T>(), array.Length);

            if (listData->Capacity < array.Length)
            {
                listData->Dispose();
                throw new Exception("Unknown Error");
            }
            listData->Length = array.Length;

            int bytesToCopy = array.Length * UnsafeCollectionUtility.SizeOf<T>();
            Buffer.MemoryCopy(array.GetUnsafePtr(), listData->Ptr, bytesToCopy, bytesToCopy);
            
            return listData;
        }

        public static UnsafeListData* Create(int sizeOf, int initialCapacity)
        {
            UnsafeListData* listData = (UnsafeListData*)Marshal.AllocHGlobal(UnsafeCollectionUtility.SizeOf<UnsafeListData>());
            listData->Length = 0;
            listData->Capacity = 0;

            if (initialCapacity != 0)
            {
                listData->SetCapacity(sizeOf, initialCapacity);
            }

            return listData;
        }

        public static void Destroy(UnsafeListData* listData)
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

        private void Dispose()
        {
            if (Ptr != null)
            {
                Marshal.FreeHGlobal((IntPtr)Ptr);
                Ptr = null;
                Length = 0;
                Capacity = 0;
            }
        }

        private void SetCapacity(int sizeOf, int capacity)
        {
            if (capacity > 0)
            {
                var itemsPerCacheLine = 64 / sizeOf;

                if (capacity < itemsPerCacheLine)
                    capacity = itemsPerCacheLine;

                capacity = UnsafeCollectionUtility.CeilPow2(capacity);
            }

            var newCapacity = capacity;
            if (newCapacity == Capacity)
                return;

            void* newPointer = null;
            if (newCapacity > 0)
            {
                var bytesToMalloc = sizeOf * newCapacity;
                newPointer = (void*)Marshal.AllocHGlobal(bytesToMalloc);

                if (Capacity > 0)
                {
                    var itemsToCopy = newCapacity < Capacity ? newCapacity : Capacity;
                    var bytesToCopy = itemsToCopy * sizeOf;
                    Buffer.MemoryCopy(Ptr, newPointer, bytesToCopy, bytesToCopy);
                }
            }

            if (Capacity > 0)
                Marshal.FreeHGlobal((IntPtr)Ptr);

            Ptr = newPointer;
            Capacity = newCapacity;
            Length = Math.Min(Length, Capacity);
        }

        public void Resize(int sizeOf, int length)
        {
#if DEBUG || ENABLE_UNITY_COLLECTIONS_CHECKS
            RequireCreated();
#endif
            var oldLength = Length;

            SetCapacity(sizeOf, length);
            Length = length;
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

        public void Add<T>(T value) where T : unmanaged
        {
            Resize(UnsafeCollectionUtility.SizeOf<T>(), Length + 1);
            WriteElement(Length - 1, value);
        }

        public void Clear()
        {
#if DEBUG || ENABLE_UNITY_COLLECTIONS_CHECKS
            RequireCreated();
#endif
            Length = 0;
        }

        public void* GetUnsafePtr()
        {
#if DEBUG || ENABLE_UNITY_COLLECTIONS_CHECKS
            RequireCreated();
#endif
            return Ptr;
        }

        public void AddRange<T>(void* ptr, int length) where T : unmanaged
        {
            AddRange(UnsafeCollectionUtility.SizeOf<T>(), ptr, length);
        }

      
        public void AddRange<T>(UnsafeListData list) where T : unmanaged
        {
            AddRange(UnsafeCollectionUtility.SizeOf<T>(), list.Ptr, list.Length);
        }

        private void AddRange(int sizeOf, void* ptr, int length)
        {
            int oldLength = Length;
            Resize(sizeOf, oldLength + length);
            void* dst = (byte*)Ptr + oldLength * sizeOf;

            int bytesToCopy = length * sizeOf;
            Buffer.MemoryCopy(ptr, dst, bytesToCopy, bytesToCopy);
        }

        public void RemoveAtSwapBack<T>(int index) where T : unmanaged
        {
            RemoveRangeSwapBack<T>(index, index + 1);
        }

        private void RemoveRangeSwapBack(int sizeOf, int begin, int end)
        {
            int itemsToRemove = end - begin;
            if (itemsToRemove > 0)
            {
                int copyFrom = Math.Max(Length - itemsToRemove, end);
                void* dst = (byte*)Ptr + begin * sizeOf;
                void* src = (byte*)Ptr + copyFrom * sizeOf;
                int bytesToCopy = Math.Min(itemsToRemove, Length - copyFrom) * sizeOf;
                Buffer.MemoryCopy(src, dst, bytesToCopy, bytesToCopy);
                Length -= itemsToRemove;
            }
        }
      
        public void RemoveRangeSwapBack<T>(int begin, int end) where T : unmanaged
        {
            RemoveRangeSwapBack(UnsafeCollectionUtility.SizeOf<T>(), begin, end);
        }

        public void RequireCreated()
        {
            if (Length > 0 && Ptr == null)
            {
                throw new System.InvalidOperationException("UnsafeArray not created");
            }
        }

        private void RequireIndexInBounds(int index)
        {
            if (index < 0 || index >= Length)
            {
                throw new System.InvalidOperationException(
                    "Index out of bounds: " + index);
            }
        }


    }
}
