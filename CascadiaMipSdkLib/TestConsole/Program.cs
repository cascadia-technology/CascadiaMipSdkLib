using CascadiaMipSdkLib;
using System;

namespace TestConsole
{
    internal class Program
    {
        internal static void Main(string[] args)
        {
            var uri = new Uri("http://xprotect");
            using (var connection = new MilestoneConnection(uri, LoginType.Windows, @"condon\jh", "AdminPW1"))
            {
                connection.IncludeChildSites = true;
                connection.Open();
                Console.WriteLine("Done");
                Console.ReadKey();
                Console.WriteLine("Logging out. . .");
            }

            Console.WriteLine("Done");
            Console.ReadKey();
        }
    }
}
