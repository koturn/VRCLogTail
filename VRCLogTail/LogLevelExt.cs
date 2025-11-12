using System;
using System.Diagnostics.CodeAnalysis;
using Koturn.VRChat.Log.Enums;


namespace VRCLogTail
{
    /// <summary>
    /// Provides extension methods for <see cref="VRCLogLevel"/>.
    /// </summary>
    public static class LogLevelExt
    {
        /// <summary>
        /// <see cref="string"/> represents of <see cref="VRCLogLevel"/>.
        /// </summary>
        private static readonly string[] _names = [
            "Other",
            "Debug",
            "Warning",
            "Error",
            "Exception"
        ];


        /// <summary>
        /// Get <see cref="string"/> represent of specified log level.
        /// </summary>
        /// <param name="level">Log level.</param>
        /// <returns><see cref="string"/> represent of <paramref name="level"/></returns>
        public static string GetName(this VRCLogLevel level)
        {
            var index = (int)level;
            if (index < 0 || index >= _names.Length)
            {
                ThrowArgumentOutOfRangeException(nameof(level), index, "Enum value is out of range");
            }
            return _names[(int)level];
        }

        /// <summary>
        /// Throws <see cref="ArgumentOutOfRangeException"/>.
        /// </summary>
        /// <param name="paramName"></param>
        /// <param name="actualValue"></param>
        /// <param name="message"></param>
        /// <exception cref="ArgumentOutOfRangeException">Always thrown.</exception>
        [DoesNotReturn]
        public static void ThrowArgumentOutOfRangeException(string paramName, int actualValue, string message)
        {
            throw new ArgumentOutOfRangeException(paramName, actualValue, message);
        }
    }
}
