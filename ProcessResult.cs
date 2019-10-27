namespace Penguin.Console
{
    /// <summary>
    /// The result of a run process
    /// </summary>
    public class ProcessResult
    {
        /// <summary>
        /// Results from the standard error stream
        /// </summary>
        public string Error { get; set; }

        /// <summary>
        /// The process exit code
        /// </summary>
        public int ExitCode { get; set; }

        /// <summary>
        /// Results from the standard output stream
        /// </summary>
        public string Output { get; set; }

        /// <summary>
        /// If true, the output and error streams were definitely fully read. If false, they may still have been but theres no
        /// guarantee because the closing characters were not found during the timeout period.
        /// </summary>
        public bool StreamRead { get; set; }

        /// <summary>
        /// The output stream if exit code 0, otherwise the error stream
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            if (ExitCode == 0)
            {
                return Output;
            }
            else
            {
                return string.IsNullOrWhiteSpace(Error) ? Output : Error;
            }
        }
    }
}