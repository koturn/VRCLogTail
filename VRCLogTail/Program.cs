using System;
using System.Globalization;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;


namespace VRCLogTail
{
    /// <summary>
    /// An entry point class.
    /// </summary>
    internal class Program
    {
        /// <summary>
        /// Cache of help message.
        /// </summary>
        private static string? _helpMessage = null;
        /// <summary>
        /// Trim characters for command.
        /// </summary>
        private static readonly char[] _commandTrimChars = [':', ' '];

        /// <summary>
        /// An entry point of this program.
        /// </summary>
        static void Main()
        {
            SetupCulture();

            Console.SetOut(new StreamWriter(Console.OpenStandardOutput(), Encoding.Default, 65536)
            {
                AutoFlush = false
            });
            Console.OutputEncoding = Encoding.UTF8;

            Console.WriteLine("Enter \":exit\", \":quit\" or EOF (Ctrl-Z) to terminate this program.");
            Console.Out.Flush();
            using (var logWatcher = new TailLogWatcher())
            {
                logWatcher.FileOpened += (sender, e) =>
                {
                    Console.WriteLine($"Start following {e.FilePath}");
                    Console.Out.Flush();
                };
                logWatcher.FileClosed += (sender, e) =>
                {
                    Console.WriteLine($"Finish following {e.FilePath}");
                    Console.Out.Flush();
                };
                logWatcher.FilterBits = 0;
                logWatcher.Start();
                logWatcher.FilterBits = TailLogWatcher.DefaultFilterBits;

                StartCommandInterpreter(logWatcher);
            }
        }

        /// <summary>
        /// Setup culture.
        /// </summary>
        private static void SetupCulture()
        {
            var culture = CultureInfo.InvariantCulture;
            CultureInfo.DefaultThreadCurrentCulture = culture;
            CultureInfo.DefaultThreadCurrentUICulture = culture;
            Thread.CurrentThread.CurrentCulture = culture;
            Thread.CurrentThread.CurrentUICulture = culture;
        }

        /// <summary>
        /// Start command interpreter.
        /// </summary>
        /// <param name="logWatcher"><see cref="TailLogWatcher"/> instance.</param>
        private static void StartCommandInterpreter(TailLogWatcher logWatcher)
        {
            string? line;
            var writer = Console.Error;
            ConsoleKeyInfo cki;
            while ((cki = Console.ReadKey(true)).KeyChar != '\x1a')
            {
                if (cki.Key == ConsoleKey.Enter)
                {
                    writer.Write('\n');
                    writer.Flush();
                }
                else if (cki.KeyChar == '/')
                {
                    writer.Write('/');
                    writer.Flush();
                    line = Console.ReadLine();
                    if (line == null)
                    {
                        break;
                    }

                    if (line.Length == 0)
                    {
                        logWatcher.FilterRegex = null;
                        writer.WriteLine("Reset filter pattern");
                    }
                    else
                    {
                        try
                        {
                            logWatcher.FilterRegex = new Regex(line, RegexOptions.CultureInvariant | RegexOptions.Compiled);
                            writer.WriteLine($"Set filter pattern=/{line}/");
                        }
#if NET5_0_OR_GREATER
                        catch (RegexParseException ex)
#else
                        catch (Exception ex)
#endif  // NET5_0_OR_GREATER
                        {
                            Console.Error.WriteLine(ex.Message);
                        }
                    }
                    continue;
                }
                else if (cki.KeyChar == ':')
                {
                    writer.Write(':');
                    writer.Flush();
                    line = Console.ReadLine();
                    if (line == null)
                    {
                        break;
                    }
                    line = line.TrimStart(_commandTrimChars).TrimEnd();
                    if (line.Length == 0)
                    {
                        continue;
                    }

                    var tokens = line.Split(' ');
                    switch (tokens[0])
                    {
                        case "cycle":
                            if (tokens.Length == 1)
                            {
                                writer.WriteLine($"cycle=[{logWatcher.WatchCycle}]");
                                continue;
                            }
                            if (tokens.Length != 2)
                            {
                                writer.WriteLine($"Invalid number of argument for \"{tokens[0]}\"");
                                continue;
                            }
                            try
                            {
                                var watchCycle = ParseNumberString(tokens[1]);
                                if (watchCycle < 0)
                                {
                                    writer.WriteLine("Cycle value must be zero or greater");
                                    continue;
                                }
                                logWatcher.WatchCycle = watchCycle;
                                writer.WriteLine($"cycle=[{logWatcher.WatchCycle}]");
                            }
                            catch
                            {
                                writer.WriteLine($"Invalid number format: {tokens[1]}");
                                continue;
                            }
                            break;
                        case "exit":
                            return;
                        case "filename":
                            if (tokens.Length == 1)
                            {
                                writer.WriteLine($"filename=[{(logWatcher.ShowFileName ? "on" : "off")}]");
                                continue;
                            }
                            if (tokens.Length != 2)
                            {
                                writer.WriteLine($"Invalid number of argument for \"{tokens[0]}\"");
                                continue;
                            }

                            switch (tokens[1].ToLower())
                            {
                                case "on":
                                case "true":
                                case "1":
                                    logWatcher.ShowFileName = true;
                                    break;
                                case "off":
                                case "false":
                                case "0":
                                    logWatcher.ShowFileName = false;
                                    break;
                                default:
                                    writer.WriteLine($"Invalid value for \"{tokens[0]}\"");
                                    continue;
                            }

                            writer.WriteLine($"filename=[{(logWatcher.ShowFileName ? "on" : "off")}]");
                            break;
                        case "help":
                            ShowHelpMessages(writer);
                            break;
                        case "level-filter":
                            if (tokens.Length == 1)
                            {
                                writer.WriteLine($"level-filter=[{logWatcher.FilterBits}]");
                                continue;
                            }
                            if (tokens.Length != 2)
                            {
                                writer.WriteLine($"Invalid number of argument for \"{tokens[0]}\"");
                                continue;
                            }

                            try
                            {
                                logWatcher.FilterBits = ParseNumberString(tokens[1]);
                                writer.WriteLine($"level-filter=[{logWatcher.FilterBits}]");
                            }
                            catch (Exception)
                            {
                                writer.WriteLine($"Invalid number format: {tokens[1]}");
                            }
                            break;
                        case "quit":
                            return;
                        default:
                            writer.WriteLine($"Unknown command: {line}");
                            break;
                    }
                }
                else if (cki.KeyChar == '?')
                {
                    ShowHelpMessages(writer);
                }
            }
        }

        /// <summary>
        /// <para>Parse string as integer number.</para>
        /// </summary>
        /// <param name="text">Number string.</param>
        /// <returns><see cref="int"/> value.</returns>
        private static int ParseNumberString(string text)
        {
            if (text.StartsWith("0b", StringComparison.InvariantCultureIgnoreCase))
            {
                return Convert.ToInt32(text.Substring(2), 2);
            }
            else if (text.StartsWith("0x", StringComparison.InvariantCultureIgnoreCase))
            {
#if NETCOREAPP2_1_OR_GREATER
                return int.Parse(text.AsSpan().Slice(2), NumberStyles.HexNumber);
#else
                return int.Parse(text.Substring(2), NumberStyles.HexNumber);
#endif  // NETCOREAPP2_1_OR_GREATER
            }
            else if (text.StartsWith("0", StringComparison.InvariantCultureIgnoreCase))
            {
                return Convert.ToInt32(text.Substring(1), 8);
            }
            else
            {
                return int.Parse(text);
            }
        }

        /// <summary>
        /// Show help message.
        /// </summary>
        /// <param name="writer"><see cref="TextWriter"/> instance to writer help message.</param>
        private static void ShowHelpMessages(TextWriter writer)
        {
            _helpMessage ??= new StringBuilder()
                .AppendLine("[Key bidings]")
                .AppendLine("  ? - Show this help.")
                .AppendLine("  /<regex-pattern> - Regex filter.")
                .AppendLine("  : - Enter command mode.")
                .AppendLine("[Commands]")
                .AppendLine("  :cycle - Show current watch cycle.")
                .AppendLine("  :cycle <number> - Set watch cycle.")
                .AppendLine("  :exit - Finish watching.")
                .AppendLine("  :filename <flag> - Show or hide file name.")
                .AppendLine("    on: Show file name")
                .AppendLine("    off: Hide file name (Default)")
                .AppendLine("  :help - Show this help.")
                .AppendLine("  :level-filter - Show current log level filter.")
                .AppendLine("  :level-filter <number> - Set log level filter bits.")
                .AppendLine("    bit 1: Debug")
                .AppendLine("    bit 2: Warning")
                .AppendLine("    bit 3: Error")
                .AppendLine("    bit 4: Exception")
                .AppendLine("    Example")
                .AppendLine("      15, 0xff or 0b1111 -> Pass all log messages. (Default)")
                .AppendLine("      14, 0xfe or 0b1110 -> Filter out debug messages, show waring, error or exception messages only.")
                .AppendLine("      0, 0x00 or 0b0000  -> Filter out all messages.")
                .AppendLine("  :quit - Finish watching. (An alias of \"exit\")")
                .ToString();
            writer.WriteLine(_helpMessage);
        }
    }
}
