using System;
using System.Collections.Generic;
using System.Text;

namespace Penguin.Console
{
    /// <summary>
    /// Static class for managing console stuff
    /// </summary>
    public static class Process
    {
        /// <summary>
        /// Runs an exe at the given path with the given args and returns the FINAL result to a string
        /// </summary>
        /// <param name="path">The path of the application to run</param>
        /// <param name="args">The application arguments</param>
        /// <returns>The full STD out as a string</returns>
        public static string Run(string path, params string[] args)
        {
            System.Diagnostics.Process process = new System.Diagnostics.Process();
            System.Diagnostics.ProcessStartInfo startInfo = new System.Diagnostics.ProcessStartInfo();

            startInfo.RedirectStandardInput = true;
            startInfo.RedirectStandardOutput = true;
            startInfo.CreateNoWindow = true;
            startInfo.UseShellExecute = false;
            startInfo.WindowStyle = System.Diagnostics.ProcessWindowStyle.Hidden;
            startInfo.FileName = path;
            startInfo.Arguments = string.Join(" ", args);
            process.StartInfo = startInfo;
            process.Start();

            process.WaitForExit();

            return process.StandardOutput.ReadToEnd();
        }
    }
}
