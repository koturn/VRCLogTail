using System;
using System.Collections.Generic;
#if NET5_0_OR_GREATER
using System.Runtime.InteropServices;
#endif  // NET5_0_OR_GREATER
#if NET9_0_OR_GREATER
using System.Threading;
#endif  // NET9_0_OR_GREATER
using Koturn.VRChat.Log;
using Koturn.VRChat.Log.Enums;
#if !NET5_0_OR_GREATER
using VRCLogTail.Internals;
#endif  // !NET5_0_OR_GREATER

#if !NET9_0_OR_GREATER
using Lock = object;
#endif  // !NET9_0_OR_GREATER


namespace VRCLogTail
{
    /// <summary>
    /// Output all logs to stdout.
    /// </summary>
    public sealed class TailLogWatcher : VRCBaseLogWatcher
    {
        /// <summary>
        /// Default log filter bits.
        /// </summary>
        public const int DefaultFilterBits = (1 << (int)VRCLogLevel.Debug)
            | (1 << (int)VRCLogLevel.Warning)
            | (1 << (int)VRCLogLevel.Error)
            | (1 << (int)VRCLogLevel.Exception);

        /// <summary>
        /// Log filter bits.
        /// </summary>
        public int FilterBits { get; set; } = DefaultFilterBits;

        /// <summary>
        /// Create an instance of <see cref="VRCBaseLogParser"/>.
        /// </summary>
        /// <param name="filePath">File path to log file.</param>
        /// <returns>An instance of <see cref="VRCBaseLogParser"/>.</returns>
        protected override VRCBaseLogParser CreateLogParser(string filePath)
        {
            return new TailLogParser(this, filePath);
        }

        /// <summary>
        /// <see cref="VRCBaseLogParser"/> for <see cref="TailLogWatcher"/>.
        /// </summary>
        /// <remarks>
        /// Primary ctor: Open specified file.
        /// </remarks>
        /// <param name="watcher"><see cref="TailLogWatcher"/> instance.</param>
        /// <param name="filePath">Log file path to open.</param>
        private sealed class TailLogParser(TailLogWatcher watcher, string filePath) : VRCBaseLogParser(filePath)
        {
            /// <summary>
            /// Lock object for <see cref="Console.Out"/>.
            /// </summary>
            private static readonly Lock _lock = new();
            /// <summary>
            /// Console colors.
            /// </summary>
            private static readonly ConsoleColor[] _consoleColor = [
                ConsoleColor.Gray,
                ConsoleColor.Gray,
                ConsoleColor.Yellow,
                ConsoleColor.Magenta,
                ConsoleColor.Red
            ];

            /// <summary>
            /// <see cref="TailLogWatcher"/> instance.
            /// </summary>
            private readonly TailLogWatcher _watcher = watcher;

            /// <summary>
            /// Load one log item and output to stdout.
            /// </summary>
            /// <param name="level">Log level.</param>
            /// <param name="logLines">Log lines.</param>
            /// <returns>True if any of the log parsing defined in this class succeeds, otherwise false.</returns>
            protected override bool OnLogDetected(VRCLogLevel level, List<string> logLines)
            {
                if ((_watcher.FilterBits & (1 << (int)level)) == 0)
                {
                    return true;
                }

                var writer = Console.Out;
#if NETCOREAPP2_1_OR_GREATER
#if NET5_0_OR_GREATER
                var logLineSpan = CollectionsMarshal.AsSpan(logLines);
#else
                var logLineSpan = ListHelper.AsSpan(logLines);
#endif  // NET5_0_OR_GREATER
                lock (_lock)
                {
                    Console.ForegroundColor = _consoleColor[(int)level];
                    writer.WriteLine($@"[{DateTimeUtil.FormatDateTime(LogAt)}][{level.GetName()}] {logLineSpan[0]}");
                    foreach (var line in logLineSpan.Slice(1))
                    {
                        writer.WriteLine(line);
                    }
                    writer.Flush();
                    Console.ForegroundColor = ConsoleColor.Gray;
                }
#else
                var logLineArray = ListHelper.GetArray(logLines);
                var count = logLines.Count;
                lock (_lock)
                {
                    Console.ForegroundColor = _consoleColor[(int)level];
                    writer.WriteLine($"[{DateTimeUtil.FormatDateTime(LogAt)}][{level.GetName()}] {logLineArray[0]}");
                    for (int i = 1; i < count; i++)
                    {
                        writer.WriteLine(logLineArray[i]);
                    }
                    writer.Flush();
                    Console.ForegroundColor = ConsoleColor.Gray;
                }
#endif  // NETCOREAPP2_1_OR_GREATER

                return true;
            }
        }
    }
}
