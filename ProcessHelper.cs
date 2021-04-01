using System;
using System.Diagnostics;
using System.Text;
using System.Threading.Tasks;

namespace Penguin.Console
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
        /// <returns>the result of the run</returns>
        public static ProcessResult Run(ProcessStartInfo startInfo, int timeout = int.MaxValue, Action<string> std = null)
        {
            StringBuilder output = new StringBuilder();
            StringBuilder error = new StringBuilder();
            ProcessResult Result = new ProcessResult();

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
                    Task Timeout = Task.Delay(15000);

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

        /// Runs an exe at the given path with the given args and returns the FINAL result to a string
        /// </summary>
        /// <param name="path">The path of the application to run</param>
        /// <param name="args">The application arguments</param>
        /// <returns>The full STD out as a string</returns>
        public static ProcessResult Run(string path, params string[] args) => Run(GetStandardStartInfo(path, args));

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