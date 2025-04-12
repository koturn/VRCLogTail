using System;
#if NETSTANDARD2_1 || NETCOREAPP2_1_OR_GREATER
using System.Runtime.CompilerServices;
#endif  // NETSTANDARD2_1 || NETCOREAPP2_1_OR_GREATER


namespace VRCLogTail
{
    /// <summary>
    /// Provides some utility methods to format <see cref="DateTime"/>.
    /// </summary>
    public static class DateTimeUtil
    {
        /// <summary>
        /// Stringify <see cref="DateTime"/> according to a format, "yyyy-MM-dd HH:mm:ss".
        /// </summary>
        /// <param name="dateTime"><see cref="DateTime"/> instance to format.</param>
        /// <returns>Datetime string.</returns>
        public static string FormatDateTime(DateTime dateTime)
        {
#if NETSTANDARD2_1 || NETCOREAPP2_1_OR_GREATER
            return string.Create(19, dateTime, (chars, dt) =>
            {
                WriteDigits(chars, 0, 4, dt.Year);
                chars[4] = '-';
                Write2Chars(chars, 5, dt.Month);
                chars[7] = '-';
                Write2Chars(chars, 8, dt.Day);
                chars[10] = ' ';
                Write2Chars(chars, 11, dt.Hour);
                chars[13] = ':';
                Write2Chars(chars, 14, dt.Minute);
                chars[16] = ':';
                Write2Chars(chars, 17, dt.Second);
            });
#else
            return dateTime.ToString("yyyy-MM-dd HH:mm:ss");
#endif  // NETSTANDARD2_1 || NETCOREAPP2_1_OR_GREATER
        }

#if NETSTANDARD2_1 || NETCOREAPP2_1_OR_GREATER
        /// <summary>
        /// Write N-digits to specified <see cref="Span{T}"/>
        /// </summary>
        /// <param name="chars">Destination <see cref="Span{T}"/>.</param>
        /// <param name="offset">Offset of span.</param>
        /// <param name="count">Number of digits to write.</param>
        /// <param name="value">Value to write.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static void WriteDigits(Span<char> chars, int offset, int count, int value)
        {
            for (int i = 0, j = offset + count - 1; i < count; i++, j--)
            {
                value = Math.DivRem(value, 10, out var rem);
                chars[j] = (char)(rem + '0');
            }
        }

        /// <summary>
        /// Write two digits to specified <see cref="Span{T}"/>
        /// </summary>
        /// <param name="chars">Destination <see cref="Span{T}"/>.</param>
        /// <param name="offset">Offset of span.</param>
        /// <param name="value">Value to write.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static void Write2Chars(Span<char> chars, int offset, int value)
        {
            value = Math.DivRem(value, 10, out var rem);
            chars[offset] = (char)(value + '0');
            chars[offset + 1] = (char)(rem + '0');
        }
#endif  // NETSTANDARD2_1 || NETCOREAPP2_1_OR_GREATER
    }
}
