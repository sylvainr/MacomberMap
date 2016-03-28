using System;
using System.Collections.Generic;
using System.Runtime.ConstrainedExecution;

namespace MacomberMapCommunications.Extensions
{
    /// <summary>
    /// Extenstion class for working with byte[]s.
    /// </summary>
    public static class ByteArrayExtensions
    {
        /// <summary>
        /// Get the array slice between the two indexes.
        /// ... Inclusive for start index, exclusive for end index.
        /// </summary>  
        public static T[] Slice<T>(this T[] source, int start, int length)
        {
            if (length <= 0)
                throw new ArgumentOutOfRangeException("length", "Length must be greater than 0.");


            // Return new array.
            T[] res = new T[length];
            for (int i = 0; i < length; i++)
            {
                res[i] = source[i + start];
            }
            return res;
        }

        /// <summary>
        /// Slice array from starting position to end.
        /// </summary>
        /// <typeparam name="T">Data Type</typeparam>
        /// <param name="source">Source Array</param>
        /// <param name="start">Starting index</param>
        /// <returns></returns>
        public static T[] Slice<T>(this T[] source, int start)
        {
            return Slice(source, start, source.Length - start);
        }

        /// <summary>
        /// Slice array from starting position to number of items.
        /// </summary>
        /// <typeparam name="T">Array Type</typeparam>
        /// <param name="source">Source Array</param>
        /// <param name="start">Starting index</param>
        /// <param name="length">Length of slice</param>
        /// <returns></returns>
        public static T[] Slice<T>(this IList<T> source, int start, int length)
        {
            if (length <= 0)
                throw new ArgumentOutOfRangeException("length", "Length must be greater than 0.");


            // Return new array.
            T[] res = new T[length];
            for (int i = 0; i < length; i++)
            {
                res[i] = source[i + start];
            }
            return res;
        }

        /// <summary>
        /// Slice list from starting position to end.
        /// </summary>
        /// <typeparam name="T">Data Type</typeparam>
        /// <param name="source">Source List</param>
        /// <param name="start">Starting index</param>
        /// <returns></returns>
        public static T[] Slice<T>(this IList<T> source, int start)
        {
            return Slice(source, start, source.Count - start);
        }

        /// <summary>
        /// Copy a portion of a byte array to another byte array.
        /// </summary>
        /// <param name="src">Source array.</param>
        /// <param name="srcIndex">Source starting position</param>
        /// <param name="dst">Destination array</param>
        /// <param name="dstIndex">Destination start index</param>
        public static void CopyByteArray(this byte[] src, int srcIndex, byte[] dst, int dstIndex)
        {
            CopyByteArray(src, srcIndex, dst, dstIndex, src.Length);
        }

        /// <summary>
        /// Copy a portion of a byte array to another byte array.
        /// </summary>
        /// <param name="src">Source array.</param>
        /// <param name="srcIndex">Source starting position</param>
        /// <param name="dst">Destination array</param>
        /// <param name="dstIndex">Destination start index</param>
        /// <param name="count">Number of bytes to copy</param>
        public static unsafe void CopyByteArray(this byte[] src, int srcIndex, byte[] dst, int dstIndex, int count)
        {
            if (src == null || srcIndex < 0 ||
                dst == null || dstIndex < 0 || count < 0)
            {
                throw new ArgumentException();
            }

            int srcLen = src.Length;
            int dstLen = dst.Length;
            if (srcLen - srcIndex < count || dstLen - dstIndex < count)
            {
                throw new ArgumentException();
            }

            // The following fixed statement pins the location of the src and dst objects
            // in memory so that they will not be moved by garbage collection.
            fixed (byte* pSrc = src, pDst = dst)
            {
                byte* ps = pSrc + srcIndex;
                byte* pd = pDst + dstIndex;

                // Loop over the end in blocks of 4 bytes, copying an integer (4 bytes) at a time:
                for (int i = 0; i < count / 4; i++)
                {
                    *((int*)pd) = *((int*)ps);
                    pd += 4;
                    ps += 4;
                }

                // Complete the copy by moving any bytes that weren't moved in blocks of 4:
                for (int i = 0; i < count % 4; i++)
                {
                    *pd = *ps;
                    pd++;
                    ps++;
                }
            }
        }

        /// <summary>
        /// Fast check if byte two arrays are equal.
        /// </summary>
        /// <param name="strA">Source array</param>
        /// <param name="strB">Test array</param>
        /// <returns></returns>
        [ReliabilityContract(Consistency.WillNotCorruptState, Cer.MayFail)]
        public static unsafe bool UnSafeEquals(this byte[] strA, byte[] strB)
        {
            int length = strA.Length;
            if (length != strB.Length)
            {
                return false;
            }
            fixed (byte* str = strA)
            {
                byte* chPtr = str;
                fixed (byte* str2 = strB)
                {
                    byte* chPtr2 = str2;
                    byte* chPtr3 = chPtr;
                    byte* chPtr4 = chPtr2;
                    while (length >= 10)
                    {
                        if ((((*(((int*)chPtr3)) != *(((int*)chPtr4))) || (*(((int*)(chPtr3 + 2))) != *(((int*)(chPtr4 + 2))))) || ((*(((int*)(chPtr3 + 4))) != *(((int*)(chPtr4 + 4)))) || (*(((int*)(chPtr3 + 6))) != *(((int*)(chPtr4 + 6)))))) || (*(((int*)(chPtr3 + 8))) != *(((int*)(chPtr4 + 8)))))
                        {
                            break;
                        }
                        chPtr3 += 10;
                        chPtr4 += 10;
                        length -= 10;
                    }
                    while (length > 0)
                    {
                        if (*(((int*)chPtr3)) != *(((int*)chPtr4)))
                        {
                            break;
                        }
                        chPtr3 += 2;
                        chPtr4 += 2;
                        length -= 2;
                    }
                    return (length <= 0);
                }
            }
        }
    }
}