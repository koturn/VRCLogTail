#if !NET5_0_OR_GREATER


using System.Collections.Generic;
using System.Runtime.CompilerServices;


namespace VRCLogTail.Internals
{
    /// <summary>
    /// Provides some methods for <see cref="List{T}"/>.
    /// </summary>
    internal static class ListHelper
    {
        /// <summary>
        /// Get internal array of <see cref="List{T}"/>.
        /// </summary>
        public static T[] GetArray<T>(List<T> list)
        {
            return Unsafe.As<ListLayout<T>>(list).items;
        }

#if NETCOREAPP2_1_OR_GREATER
        /// <summary>
        /// Get <see cref="Span{T}"/> of internal array of <see cref="List{T}"/>.
        /// </summary>
        public static Span<T> AsSpan<T>(List<T> list)
        {
            return Unsafe.As<ListLayout<T>>(list).items;
        }
#endif  // NETCOREAPP2_1_OR_GREATER

        /// <summary>
        /// List layout class
        /// </summary>
        /// <typeparam name="T">Type of array elements.</typeparam>
        private class ListLayout<T>
        {
            /// <summary>
            /// Internal array of list.
            /// </summary>
            internal T[] items = [];
        }
    }
}


#endif  // !NET5_0_OR_GREATER
