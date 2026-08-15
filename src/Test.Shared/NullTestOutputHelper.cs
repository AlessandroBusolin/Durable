namespace Test.Shared
{
    using Xunit.Abstractions;

    /// <summary>
    /// An <see cref="ITestOutputHelper"/> that discards output, used when instantiating xUnit test classes
    /// through the Touchstone bridge (where no live xUnit output sink exists).
    /// </summary>
    internal sealed class NullTestOutputHelper : ITestOutputHelper
    {
        /// <summary>
        /// Discards the supplied message.
        /// </summary>
        /// <param name="message">The message to discard.</param>
        public void WriteLine(string message)
        {
        }

        /// <summary>
        /// Discards the supplied formatted message.
        /// </summary>
        /// <param name="format">The format string.</param>
        /// <param name="args">The format arguments.</param>
        public void WriteLine(string format, params object[] args)
        {
        }
    }
}
