using System;
using System.Collections.Generic;
#if NET5_0_OR_GREATER
using System.Runtime.InteropServices;
#endif  // NET5_0_OR_GREATER
using Koturn.VRChat.Log;
using Koturn.VRChat.Log.Enums;


namespace VRCLogTail
{
    /// <summary>
    /// Output all logs to stdout.
    /// </summary>
    public sealed class TailLogWatcher : VRCBaseLogWatcher
    {
        /// <summary>
        /// Create an instance of <see cref="VRCBaseLogParser"/>.
        /// </summary>
        /// <param name="filePath">File path to log file.</param>
        /// <returns>An instance of <see cref="VRCBaseLogParser"/>.</returns>
        protected override VRCBaseLogParser CreateLogParser(string filePath)
        {
            return new TailLogParser(filePath);
        }

        /// <summary>
        /// <see cref="VRCBaseLogParser"/> for <see cref="TailLogWatcher"/>.
        /// </summary>
        /// <remarks>
        /// Primary ctor: Open specified file.
        /// </remarks>
        /// <param name="filePath">Log file path to open.</param>
        private sealed class TailLogParser(string filePath) : VRCBaseLogParser(filePath)
        {
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
            /// Load one log item and output to stdout.
            /// </summary>
            /// <param name="logAt">Log timestamp.</param>
            /// <param name="level">Log level.</param>
            /// <param name="logLines">Log lines.</param>
            /// <returns>True if any of the log parsing defined in this class succeeds, otherwise false.</returns>
            protected override bool OnLogDetected(DateTime logAt, LogLevel level, List<string> logLines)
            {
                Console.ForegroundColor = _consoleColor[(int)level];

                var writer = Console.Out;
#if NET5_0_OR_GREATER
                var logLineSpan = CollectionsMarshal.AsSpan(logLines);
                writer.WriteLine($@"[{DateTimeUtil.FormatDateTime(logAt)}][{level.GetName()}] {logLineSpan[0]}");
                foreach (var line in logLineSpan.Slice(1))
                {
                    writer.WriteLine(line);
                }
#else
                writer.WriteLine($"[{DateTimeUtil.FormatDateTime(logAt)}][{level.GetName()}] {logLines[0]}");
                var count = logLines.Count;
                for (int i = 0; i < count; i++)
                {
                    writer.WriteLine(logLines[i]);
                }
#endif  // NET5_0_OR_GREATER
                writer.Flush();

                Console.ForegroundColor = ConsoleColor.Gray;

                return true;
            }
        }
    }
}
