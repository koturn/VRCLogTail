using System;
using System.IO;
using System.Text;


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
                logWatcher.Start();
                while (Console.ReadLine() != "exit")
                {
                    // Do nothing.
                }
            }
        }
    }
}
