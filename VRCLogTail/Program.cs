using System;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;


namespace VRCLogTail
{
    /// <summary>
    /// An entry point class.
    /// </summary>
    internal class Program
    {
        /// <summary>
        /// An entry point of this program.
        /// </summary>
        static void Main()
        {
            Console.SetOut(new StreamWriter(Console.OpenStandardOutput(), Encoding.Default, 65536)
            {
                AutoFlush = false
            });
            Console.OutputEncoding = Encoding.UTF8;

            using (var logWatcher = new TailLogWatcher())
            {
                logWatcher.FilterBits = 0;
                logWatcher.Start();
                logWatcher.FilterBits = TailLogWatcher.DefaultFilterBits;

                string? line;
                while ((line = Console.ReadLine()) != null)
                {
                    line = line.Trim();
                    if (line == "exit")
                    {
                        break;
                    }

#if NETCOREAPP2_0_OR_GREATER || NETSTANDARD2_1_OR_GREATER
                    if (line.StartsWith('/'))
#else
                    if (line.StartsWith("/"))
#endif  // NETCOREAPP2_0_OR_GREATER || NETSTANDARD2_1
                    {
                        if (line.Length == 1)
                        {
                            logWatcher.FilterRegex = null;
                        }
                        else
                        {
                            try
                            {
                                logWatcher.FilterRegex = new Regex(line.Substring(1), RegexOptions.CultureInvariant | RegexOptions.Compiled);
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

                    var tokens = line.Split(' ');
                    if (tokens[0] == "cycle")
                    {
                        if (tokens.Length == 1)
                        {
                            Console.Error.WriteLine($"cycle=[{logWatcher.WatchCycle}]");
                            continue;
                        }
                        if (tokens.Length != 2)
                        {
                            Console.Error.WriteLine($"Invalid number of argument for \"{tokens[0]}\"");
                            continue;
                        }
                        if (!int.TryParse(tokens[1], out var watchCycle))
                        {
                            Console.Error.WriteLine($"Invalid number format: {tokens[1]}");
                            continue;
                        }
                        if (watchCycle < 0)
                        {
                            Console.Error.WriteLine("Cycle value must be zero or greater");
                            continue;
                        }
                        logWatcher.WatchCycle = watchCycle;
                    }
                    else if (tokens[0] == "level-filter")
                    {
                        if (tokens.Length == 1)
                        {
                            Console.Error.WriteLine($"level-filter=[{logWatcher.FilterBits}]");
                            continue;
                        }
                        if (tokens.Length != 2)
                        {
                            Console.Error.WriteLine($"Invalid number of argument for \"{tokens[0]}\"");
                            continue;
                        }
                        if (!int.TryParse(tokens[1], out var filterBits))
                        {
                            Console.Error.WriteLine($"Invalid number format: {tokens[1]}");
                            continue;
                        }
                        logWatcher.FilterBits = filterBits;
                    }
                }
            }
        }
    }
}
