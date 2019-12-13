namespace AiNav.Collections
{
    public static unsafe class UnsafeCollectionUtility
    {
        public static int SizeOf<T>() where T : unmanaged
        {
            return sizeof(T);
        }

        public static int CeilPow2(int i)
        {
            i -= 1;
            i |= i >> 1;
            i |= i >> 2;
            i |= i >> 4;
            i |= i >> 8;
            i |= i >> 16;
            return i + 1;
        }

        public static T ReadArrayElement<T>(void* ptr, int index) where T : unmanaged
        {
            T* array = (T*)ptr;
            return array[index];
        }

        public static void WriteArrayElement<T>(void* ptr, int index, T value) where T : unmanaged
        {
            T* array = (T*)ptr;
            array[index] = value;
        }

        

        
    }
}
