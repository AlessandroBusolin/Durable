namespace Test.Nunit
{
    using System.Collections;
    using System.Threading;
    using System.Threading.Tasks;
    using NUnit.Framework;
    using Test.Shared;
    using Touchstone.Core;
    using Touchstone.NunitAdapter;

    /// <summary>
    /// Surfaces every Durable Touchstone test case as an NUnit test. The test bodies live in Test.Shared;
    /// this class only adapts them to the NUnit runner so they are visible to <c>dotnet test</c> and Test Explorer.
    /// </summary>
    [TestFixture]
    [NonParallelizable]
    public sealed class DurableTouchstoneNunitTests
    {
        private static IEnumerable TestCases()
        {
            return new TouchstoneTestCaseSource(DurableTestSuites.All);
        }

        /// <summary>
        /// Executes a single Touchstone test case.
        /// </summary>
        /// <param name="testCase">The case to execute.</param>
        /// <returns>A task representing the asynchronous test execution.</returns>
        [Test]
        [TestCaseSource(nameof(TestCases))]
        public async Task RunTest(TestCaseDescriptor testCase)
        {
            await testCase.ExecuteAsync(CancellationToken.None);
        }
    }
}
