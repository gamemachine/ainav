/*
 *  Copyright (c) 2018 Stanislav Denisov
 *
 *  Permission is hereby granted, free of charge, to any person obtaining a copy
 *  of this software and associated documentation files (the "Software"), to deal
 *  in the Software without restriction, including without limitation the rights
 *  to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
 *  copies of the Software, and to permit persons to whom the Software is
 *  furnished to do so, subject to the following conditions:
 *
 *  The above copyright notice and this permission notice shall be included in all
 *  copies or substantial portions of the Software.
 *
 *  THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
 *  IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
 *  FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
 *  AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
 *  LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
 *  OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
 *  SOFTWARE.
 */

using System;
using System.Runtime.CompilerServices;

namespace AiNav.Collections
{
    public static unsafe class MemoryCopy
    {
        public static void Copy<T>(void* source, T[] destination, int length) where T : unmanaged
        {
            if (length > 0)
            {
                fixed (T* destinationPointer = &destination[0])
                {

                    System.Buffer.MemoryCopy(source, destinationPointer, length, length);
                }
            }
        }

        public static void Copy<T>(T[] source, int sourceOffset, void* destination, int destinationOffset, int length) where T : unmanaged
        {
            if (length > 0)
            {
                fixed (T* sourcePointer = &source[sourceOffset])
                {
                    T* destinationPointer = (T*)destination + destinationOffset;

                    Buffer.MemoryCopy(sourcePointer, destinationPointer, length, length);
                }
            }
        }

        [MethodImpl(256)]
        public static void Copy(void* source, int sourceOffset, byte[] destination, int destinationOffset, int length)
        {
            if (length > 0)
            {
                fixed (byte* destinationPointer = &destination[destinationOffset])
                {
                    byte* sourcePointer = (byte*)source + sourceOffset;

                    Buffer.MemoryCopy(sourcePointer, destinationPointer, length, length);
                }
            }
        }

        [MethodImpl(256)]
        public static void Copy(byte[] source, int sourceOffset, void* destination, int destinationOffset, int length)
        {
            if (length > 0)
            {
                fixed (byte* sourcePointer = &source[sourceOffset])
                {
                    byte* destinationPointer = (byte*)destination + destinationOffset;

                    Buffer.MemoryCopy(sourcePointer, destinationPointer, length, length);
                }
            }
        }

        [MethodImpl(256)]
        public static void Copy(byte[] source, int sourceOffset, IntPtr destination, int destinationOffset, int length)
        {
            if (length > 0)
            {
                fixed (byte* sourcePointer = &source[sourceOffset])
                {
                    byte* destinationPointer = (byte*)destination + destinationOffset;

                    Buffer.MemoryCopy(sourcePointer, destinationPointer, length, length);
                }
            }
        }

        [MethodImpl(256)]
        public static void Copy(byte[] source, int sourceOffset, byte[] destination, int destinationOffset, int length)
        {
            if (length > 0)
            {
                fixed (byte* sourcePointer = &source[sourceOffset])
                {
                    fixed (byte* destinationPointer = &destination[destinationOffset])
                    {
                        Buffer.MemoryCopy(sourcePointer, destinationPointer, length, length);
                    }
                }
            }
        }

        public static void Copy<T>(byte[] source, T[] destination) where T : unmanaged
        {
            if (source.Length > 0)
            {
                fixed (byte* sourcePointer = &source[0])
                {
                    fixed (T* destinationPointer = &destination[0])
                    {
                        Buffer.MemoryCopy(sourcePointer, destinationPointer, source.Length, source.Length);
                    }
                }
            }
        }

        public static void Copy<T>(T[] source, byte[] destination) where T : unmanaged
        {
            if (destination.Length > 0)
            {
                fixed (T* sourcePointer = &source[0])
                {
                    fixed (byte* destinationPointer = &destination[0])
                    {
                        Buffer.MemoryCopy(sourcePointer, destinationPointer, destination.Length, destination.Length);
                    }
                }
            }
        }

    }
}