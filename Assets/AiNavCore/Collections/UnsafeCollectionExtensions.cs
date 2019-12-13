using System;

namespace AiNav.Collections
{
    public static unsafe class UnsafeCollectionExtensions
    {

        public static byte[] ToByteArray<T>(this T[] source) where T : unmanaged
        {
            int sizeInBytes = source.Length * UnsafeCollectionUtility.SizeOf<T>();
            byte[] target = new byte[sizeInBytes];
            MemoryCopy.Copy(source, target);
            return target;
        }

        public static T[] ToArray<T>(this byte[] source) where T : unmanaged
        {
            if (source.Length % UnsafeCollectionUtility.SizeOf<T>() != 0)
            {
                throw new ArgumentException("Invalid length for type");
            }

            int size = source.Length / UnsafeCollectionUtility.SizeOf<T>();
            T[] target = new T[size];
            MemoryCopy.Copy<T>(source, target);
            return target;
        }

        public static AiNativeArray<T> ToNativeArray<T>(this T[] source) where T : unmanaged
        {
            AiNativeArray<T> destination = new AiNativeArray<T>(source.Length);
            MemoryCopy.Copy(source, 0, destination.GetUnsafePtr(), 0, destination.SizeOf);
            return destination;
        }

        public static AiNativeArray<T> ToNativeArray<T>(this byte[] source) where T : unmanaged
        {
            if (source.Length % UnsafeCollectionUtility.SizeOf<T>() != 0)
            {
                throw new ArgumentException("Invalid length for type");
            }

            int size = source.Length / UnsafeCollectionUtility.SizeOf<T>();
            AiNativeArray<T> destination = new AiNativeArray<T>(size);
            MemoryCopy.Copy(source, 0, destination.GetUnsafePtr(), 0, source.Length);
            return destination;
        }

        public static AiNativeList<T> ToNativeList<T>(this byte[] source) where T : unmanaged
        {
            var data = UnsafeListData.CreateFrom<T>(source);
            return new AiNativeList<T>(data);
        }

        public static byte[] ToByteArray<T>(this AiNativeArray<T> array) where T : unmanaged
        {
            byte[] target = new byte[array.SizeOf];
            MemoryCopy.Copy(array.GetUnsafePtr(), 0, target, 0, array.SizeOf);
            return target;
        }

        public static byte[] ToByteArray<T>(this AiNativeList<T> list) where T : unmanaged
        {
            int bytesToCopy = list.Length * UnsafeCollectionUtility.SizeOf<T>();
            byte[] target = new byte[bytesToCopy];
            MemoryCopy.Copy(list.GetUnsafePtr(), 0, target, 0, bytesToCopy);
            return target;
        }

        public static T[] ToArray<T>(this AiNativeArray<T> array) where T : unmanaged
        {
            T[] target = new T[array.Length];
            MemoryCopy.Copy(array.GetUnsafePtr(), target, array.SizeOf);
            return target;
        }

        public static void CopyTo<T>(this AiNativeArray<T> array, T[] target) where T : unmanaged
        {
            if (target.Length < array.Length)
            {
                throw new ArgumentException("Destination array is smaller then source array");
            }
            MemoryCopy.Copy(array.GetUnsafePtr(), target, array.SizeOf);
        }

        public static T[] ToArray<T>(this AiNativeList<T> list) where T : unmanaged
        {
            T[] array = new T[list.Length];
            int length = list.Length * UnsafeCollectionUtility.SizeOf<T>();
            MemoryCopy.Copy((T*)list.GetUnsafePtr(), array, length);
            return array;
        }

        public static bool Contains<T, U>(this AiNativeList<T> list, U value) where T : unmanaged, IEquatable<U>
        {
            return IndexOf<T, U>(list.GetUnsafePtr(), list.Length, value) != -1;
        }
      
        public static int IndexOf<T, U>(this AiNativeList<T> list, U value) where T : unmanaged, IEquatable<U>
        {
            return IndexOf<T, U>(list.GetUnsafePtr(), list.Length, value);
        }

        public static bool Contains<T, U>(void* ptr, int length, U value) where T : unmanaged, IEquatable<U>
        {
            return IndexOf<T, U>(ptr, length, value) != -1;
        }

        public static int IndexOf<T, U>(void* ptr, int length, U value) where T : unmanaged, IEquatable<U>
        {
            for (int i = 0; i != length; i++)
            {
                if (UnsafeCollectionUtility.ReadArrayElement<T>(ptr, i).Equals(value))
                    return i;
            }
            return -1;
        }

        
    }
}
