using System;
using System.Diagnostics;
using System.Text;
using System.Threading.Tasks;

namespace Penguin.PgConsole
{
    /// <summary>
    /// Static class for managing console stuff
    /// </summary>
    public static class ProcessHelper
    {
        /// <summary>
        /// Starts a process with the given startinfo
        /// </summary>
        /// <param name="startInfo">the startinfo of the process</param>
        /// <param name="timeout"></param>
        /// <param name="std"></param>
        /// <returns>the result of the run</returns>
        public static ProcessResult Run(ProcessStartInfo startInfo, int timeout = 15000, Action<string> std = null)
        {
            if (startInfo is null)
            {
                throw new ArgumentNullException(nameof(startInfo));
            }

            StringBuilder output = new StringBuilder();
            StringBuilder error = new StringBuilder();
            ProcessResult Result = new ProcessResult();

            startInfo.RedirectStandardError = true;
            startInfo.RedirectStandardOutput = true;
            
            using (Process process = new Process())
            {
                process.StartInfo = startInfo;

                TaskCompletionSource<bool> StreamDone = new TaskCompletionSource<bool>();

                bool outputDone = false;
                bool errorDone = false;

                void HandleData(StringBuilder sb, DataReceivedEventArgs e, ref bool done)
                {
                    if (e.Data is null)
                    {
                        done = true;

                        if (errorDone && outputDone && !StreamDone.Task.IsCompleted)
                        {
                            StreamDone.SetResult(true);
                        }
                    }
                    else
                    {
                        std?.Invoke(e.Data + System.Environment.NewLine);
                        sb.Append(e.Data);
                    }

                    sb.Append(System.Environment.NewLine);

                    
                }

                process.OutputDataReceived += (sender, e) => HandleData(output, e, ref outputDone);

                process.ErrorDataReceived += (sender, e) => HandleData(error, e, ref errorDone);

                //Task<string> stdOutTask = process.StandardOutput.ReadToEndAsync();
                //Task<string> stdErrTask = process.StandardError.ReadToEndAsync();

                Task WaitShort = new Task(() =>
                {
                    while (!process.WaitForExit(5000) && !process.HasExited)
                    {
                    }
                });

                Task WaitLong = new Task(() => process.WaitForExit());               

                Task TryFinish = new Task(() =>
                {
                    WaitLong.Start();
                    Task Timeout = Task.Delay(timeout);

                    Task.WaitAny(WaitLong, Timeout);
                });

                WaitShort.ContinueWith((a) => TryFinish.Start());

                process.Start();

                WaitShort.Start();

                process.BeginOutputReadLine();
                process.BeginErrorReadLine();

                Task.WaitAny(TryFinish, StreamDone.Task);

                Result.ExitCode = process.ExitCode;

                process.Close();

                Result.Output = output.ToString();
                Result.Error = error.ToString();
            }

            return Result;
        }

        /// <summary>
        /// Runs an exe at the given path with the given args and returns the FINAL result to a string
        /// </summary>
        /// <param name="path">The path of the application to run</param>
        /// <param name="args">The application arguments</param>
        /// <returns></returns>
        public static ProcessResult Run(string path, params string[] args) => Run(GetStandardStartInfo(path, args));

        /// <summary>
        /// Runs an exe at the given path with the given args and returns the FINAL result to a string
        /// </summary>
        /// <param name="path">The path of the application to run</param>
        /// <param name="args">The application arguments</param>
        /// <param name="Std">Delegate to invoke when a new line is returned/param>
        /// <returns></returns>
        public static ProcessResult Run(string path, Action<string> Std, params string[] args) => Run(GetStandardStartInfo(path, args), int.MaxValue, Std);

        /// <summary>
        /// Runs an exe at the given path with the given args and returns the FINAL result to a string
        /// </summary>
        /// <param name="path">The path of the application to run</param>
        /// <param name="workingDirectory">The working directory of the command</param>
        /// <param name="args">The application arguments</param>
        /// <returns>The full STD out as a string</returns>
        public static ProcessResult RunIn(string path, string workingDirectory, Action<string> Std, params string[] args)
        {
            System.Diagnostics.ProcessStartInfo startInfo = GetStandardStartInfo(path, args);

            startInfo.WorkingDirectory = workingDirectory;

            return Run(startInfo, int.MaxValue, Std);
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
            System.Diagnostics.ProcessStartInfo startInfo = new System.Diagnostics.ProcessStartInfo
            {
                RedirectStandardInput = true,
                RedirectStandardOutput = true,
                RedirectStandardError = true,

                CreateNoWindow = true,
                UseShellExecute = false,
                WindowStyle = System.Diagnostics.ProcessWindowStyle.Hidden,
                FileName = path,
                Arguments = string.Join(" ", args)
            };
            return startInfo;
        }
    }
}