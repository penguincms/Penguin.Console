using System.Diagnostics;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Penguin.Console
{
    /// <summary>
    /// Static class for managing console stuff
    /// </summary>
    public static class Process
    {
        /// <summary>
        /// Starts a process with the given startinfo
        /// </summary>
        /// <param name="startInfo">the startinfo of the process</param>
        /// <returns>the result of the run</returns>
        public static ProcessResult Run(System.Diagnostics.ProcessStartInfo startInfo, int timeout = int.MaxValue)
        {
            StringBuilder output = new StringBuilder();
            StringBuilder error = new StringBuilder();
            ProcessResult Result = new ProcessResult();

            using (System.Diagnostics.Process process = new System.Diagnostics.Process())
            {
                process.StartInfo = startInfo;
                bool outputDone = false;
                bool errorDone = false;
                process.OutputDataReceived += (sender, e) => { if (e.Data is null) { outputDone = true; } else { output.Append(e.Data); } output.Append(System.Environment.NewLine); };
                process.ErrorDataReceived += (sender, e) => { if (e.Data is null) { errorDone = true; } else { error.Append(e.Data); } error.Append(System.Environment.NewLine); };
                process.Start();
                process.BeginOutputReadLine();
                process.BeginErrorReadLine();

                //Task<string> stdOutTask = process.StandardOutput.ReadToEndAsync();
                //Task<string> stdErrTask = process.StandardError.ReadToEndAsync();

                bool ProcessDone()
                {
                    return process.HasExited || process.WaitForExit(5000);
                }

                bool StreamDone()
                {
                    return errorDone && outputDone;
                }

                while (!ProcessDone() || !StreamDone())
                {
                    if (ProcessDone())
                    {
                        //If the process is done and we havent gotten new stream information a whole second
                        //later, we probably arent ever getting it
                        System.Threading.Thread.Sleep(1000);

                        break;
                    }
                }

                Result.ExitCode = process.ExitCode;

                process.Close();

                Result.Output = output.ToString();
                Result.Error = error.ToString();
            }

            return Result;
        }

        /// Runs an exe at the given path with the given args and returns the FINAL result to a string
        /// </summary>
        /// <param name="path">The path of the application to run</param>
        /// <param name="args">The application arguments</param>
        /// <returns>The full STD out as a string</returns>
        public static ProcessResult Run(string path, params string[] args)
        {
            return Run(GetStandardStartInfo(path, args));
        }

        /// <summary>
        /// Runs an exe at the given path with the given args and returns the FINAL result to a string
        /// </summary>
        /// <param name="path">The path of the application to run</param>
        /// <param name="workingDirectory">The working directory of the command</param>
        /// <param name="args">The application arguments</param>
        /// <returns>The full STD out as a string</returns>
        public static ProcessResult RunIn(string path, string workingDirectory, params string[] args)
        {
            System.Diagnostics.ProcessStartInfo startInfo = GetStandardStartInfo(path, args);

            startInfo.WorkingDirectory = workingDirectory;

            return Run(startInfo);
        }

        internal static System.Diagnostics.ProcessStartInfo GetStandardStartInfo(string path, params string[] args)
        {
            System.Diagnostics.ProcessStartInfo startInfo = new System.Diagnostics.ProcessStartInfo();
            startInfo.RedirectStandardInput = true;
            startInfo.RedirectStandardOutput = true;
            startInfo.RedirectStandardError = true;

            startInfo.CreateNoWindow = true;
            startInfo.UseShellExecute = false;
            startInfo.WindowStyle = System.Diagnostics.ProcessWindowStyle.Hidden;
            startInfo.FileName = path;
            startInfo.Arguments = string.Join(" ", args);
            return startInfo;
        }
    }
}