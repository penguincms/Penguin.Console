using System.Diagnostics;
using System.Text;
using System.Threading.Tasks;

namespace Penguin.PgConsole
{
    // based on https://gist.github.com/AlexMAS/276eed492bc989e13dcce7c78b9e179d
    internal static class ProcessAsyncHelper
    {
        public struct ProcessResult
        {
            public string Error;
            public int? ExitCode;
            public string Output;
        }

        public static async Task<ProcessResult> RunProcessAsync(ProcessStartInfo pstartInfo, int timeout)
        {
            ProcessResult result = new ProcessResult();

            using (Process process = new Process())
            {
                process.StartInfo = pstartInfo;

                StringBuilder outputBuilder = new StringBuilder();
                TaskCompletionSource<bool> outputCloseEvent = new TaskCompletionSource<bool>();

                process.OutputDataReceived += (s, e) =>
                {
                    if (e.Data == null)
                    {
                        outputCloseEvent.SetResult(true);
                    }
                    else
                    {
                        outputBuilder.Append(e.Data);
                    }
                };

                StringBuilder errorBuilder = new StringBuilder();
                TaskCompletionSource<bool> errorCloseEvent = new TaskCompletionSource<bool>();

                process.ErrorDataReceived += (s, e) =>
                {
                    if (e.Data == null)
                    {
                        errorCloseEvent.SetResult(true);
                    }
                    else
                    {
                        errorBuilder.Append(e.Data);
                    }
                };

                bool isStarted = process.Start();
                if (!isStarted)
                {
                    result.ExitCode = process.ExitCode;
                    return result;
                }

                // Reads the output stream first and then waits because deadlocks are possible
                process.BeginOutputReadLine();
                process.BeginErrorReadLine();

                // Creates task to wait for process exit using timeout
                Task<bool> waitForExit = WaitForExitAsync(process, timeout);

                // Create task to wait for process exit and closing all output streams
                Task<bool[]> processTask = Task.WhenAll(waitForExit, outputCloseEvent.Task, errorCloseEvent.Task);

                // Waits process completion and then checks it was not completed by timeout
                if (await Task.WhenAny(Task.Delay(timeout), processTask) == processTask && waitForExit.Result)
                {
                    result.ExitCode = process.ExitCode;
                    result.Output = outputBuilder.ToString();
                    result.Error = errorBuilder.ToString();
                }
                else
                {
                    try
                    {
                        // Kill hung process
                        process.Kill();
                    }
                    catch
                    {
                        // ignored
                    }
                }
            }

            return result;
        }

        private static Task<bool> WaitForExitAsync(Process process, int timeout) => Task.Run(() => process.WaitForExit(timeout));
    }
}