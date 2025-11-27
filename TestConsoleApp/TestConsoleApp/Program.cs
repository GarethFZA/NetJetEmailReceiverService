using System;
using Charismatech.MessageQueueClasses;
using System.Threading;

namespace EmailProcessorConsoleTest
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Email processor console test");
            Console.WriteLine("Type 'run' to run once, 'loop' to run repeatedly, or press Enter to run once.");
            var cmd = Console.ReadLine();
            bool loop = string.Equals(cmd, "loop", StringComparison.OrdinalIgnoreCase);

            do
            {
                try
                {
                    Console.WriteLine($"Starting ProcessEmailsIMAP at {DateTime.Now:O}");
                    MessageReceiveSingleton.Instance.ProcessEmailsIMAP();
                    Console.WriteLine($"Finished ProcessEmailsIMAP at {DateTime.Now:O}");
                }
                catch (Exception ex)
                {
                    // Avoid throwing — surface exception to console for debugging
                    Console.WriteLine("Unhandled exception while processing emails:");
                    Console.WriteLine(ex.ToString());
                }

                if (!loop)
                {
                    Console.WriteLine("Run complete. Type 'run' and press Enter to run again, 'loop' to run continuously, or 'exit' to quit.");
                    var next = Console.ReadLine();
                    if (string.Equals(next, "exit", StringComparison.OrdinalIgnoreCase)) break;
                    if (string.Equals(next, "loop", StringComparison.OrdinalIgnoreCase)) loop = true;
                    if (!string.Equals(next, "run", StringComparison.OrdinalIgnoreCase) && string.IsNullOrEmpty(next)) break;
                }
                else
                {
                    // configurable delay between loop iterations
                    Thread.Sleep(TimeSpan.FromMinutes(1));
                }
            } while (true);

            Console.WriteLine("Exiting.");
        }
    }
}