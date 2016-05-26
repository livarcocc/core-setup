using System;
using System.Diagnostics;
using System.IO;
using System.Reflection;

namespace Dotnet.Deb.Tool
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var processInfo = new ProcessStartInfo()
            {
                FileName = Path.Combine(typeof(Program).GetTypeInfo().Assembly.Location, "tool", "package_tool"),
                Arguments = ArgumentEscaper.EscapeAndConcatenateArgArrayForProcessStart(args),
                UseShellExecute = false
            };

            var process = new Process()
            {
                StartInfo = processInfo
            };

            process.Start();
            process.WaitForExit();
        }
    }
}
