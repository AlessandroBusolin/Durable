namespace Test.Xunit
{
    using System.Threading;
    using System.Threading.Tasks;
    using Test.Shared;
    using Touchstone.Core;
    using global::Xunit;

    /// <summary>
    /// Surfaces every Durable Touchstone test case as an xUnit theory. The test bodies live in Test.Shared;
    /// this class only adapts them to the xUnit runner so they are visible to <c>dotnet test</c> and Test Explorer.
    /// </summary>
    public sealed class DurableTouchstoneXunitTests
    {
        /// <summary>
        /// Enumerates all non-skipped Touchstone cases for the configured provider (SQLite by default).
        /// </summary>
        /// <returns>Theory data containing one entry per test case.</returns>
        public static TheoryData<TestCaseDescriptor> TestCases()
        {
            TheoryData<TestCaseDescriptor> data = new TheoryData<TestCaseDescriptor>();

            foreach (TestSuiteDescriptor suite in DurableTestSuites.All)
            {
                foreach (TestCaseDescriptor testCase in suite.Cases)
                {
                    if (!testCase.Skip)
                    {
                        data.Add(testCase);
                    }
                }
            }

            return data;
        }

        /// <summary>
        /// Executes a single Touchstone test case.
        /// </summary>
        /// <param name="testCase">The case to execute.</param>
        /// <returns>A task representing the asynchronous test execution.</returns>
        [Theory]
        [MemberData(nameof(TestCases))]
        public async Task RunTest(TestCaseDescriptor testCase)
        {
            await testCase.ExecuteAsync(CancellationToken.None);
        }
    }
}
