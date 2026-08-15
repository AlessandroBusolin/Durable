namespace Test.Shared
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;

    /// <summary>
    /// Holds the shared repository provider and configuration used by every Touchstone test case.
    /// The provider's schema is created exactly once per process (idempotently) the first time a case runs,
    /// mirroring the single setup/teardown lifecycle used by the legacy shared test runner.
    /// Thread-safe: initialization is guarded by an async-aware lock.
    /// </summary>
    public static class DurableTestRuntime
    {
        #region Private-Members

        private static readonly object _SyncRoot = new object();
        private static readonly SemaphoreSlim _InitLock = new SemaphoreSlim(1, 1);
        private static TestRuntimeConfiguration _Configuration = LoadDefaultConfiguration();
        private static IRepositoryProvider? _Provider;
        private static bool _Initialized;

        #endregion

        #region Public-Members

        /// <summary>
        /// Gets a copy of the current runtime configuration.
        /// </summary>
        public static TestRuntimeConfiguration Configuration
        {
            get
            {
                lock (_SyncRoot)
                {
                    return _Configuration.Copy();
                }
            }
        }

        #endregion

        #region Public-Methods

        /// <summary>
        /// Replaces the runtime configuration. Must be called before any test case executes.
        /// </summary>
        /// <param name="configuration">The configuration to apply. Cannot be null.</param>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="configuration"/> is null.</exception>
        /// <exception cref="InvalidOperationException">Thrown when configuration changes after initialization.</exception>
        public static void Configure(TestRuntimeConfiguration configuration)
        {
            if (configuration == null) throw new ArgumentNullException(nameof(configuration));

            lock (_SyncRoot)
            {
                if (_Initialized)
                {
                    throw new InvalidOperationException("DurableTestRuntime cannot be reconfigured after initialization.");
                }

                _Configuration = configuration.Copy();
            }
        }

        /// <summary>
        /// Ensures the shared provider has been created and its schema initialized. Idempotent and thread-safe.
        /// </summary>
        /// <param name="token">A cancellation token.</param>
        /// <returns>The shared repository provider.</returns>
        public static async Task<IRepositoryProvider> EnsureInitializedAsync(CancellationToken token = default)
        {
            if (_Initialized && _Provider != null)
            {
                return _Provider;
            }

            await _InitLock.WaitAsync(token).ConfigureAwait(false);
            try
            {
                if (_Initialized && _Provider != null)
                {
                    return _Provider;
                }

                TestRuntimeConfiguration configuration;
                lock (_SyncRoot)
                {
                    configuration = _Configuration.Copy();
                }

                IRepositoryProvider provider = RepositoryProviderFactory.Create(configuration);
                await provider.SetupDatabaseAsync().ConfigureAwait(false);

                lock (_SyncRoot)
                {
                    _Provider = provider;
                    _Initialized = true;
                }

                return provider;
            }
            finally
            {
                _InitLock.Release();
            }
        }

        /// <summary>
        /// Returns the already-initialized shared provider. Intended for synchronous instance factories that run
        /// after <see cref="EnsureInitializedAsync"/> has completed (e.g., a Touchstone before-each hook).
        /// </summary>
        /// <returns>The initialized shared repository provider.</returns>
        /// <exception cref="InvalidOperationException">Thrown when the provider has not been initialized yet.</exception>
        public static IRepositoryProvider RequireProvider()
        {
            lock (_SyncRoot)
            {
                if (_Provider == null)
                {
                    throw new InvalidOperationException("The shared provider has not been initialized. Ensure EnsureInitializedAsync has run first.");
                }

                return _Provider;
            }
        }

        /// <summary>
        /// Cleans up the shared provider's schema and disposes it. Safe to call multiple times.
        /// </summary>
        /// <returns>A task representing the asynchronous cleanup operation.</returns>
        public static async Task CleanupAsync()
        {
            IRepositoryProvider? provider;
            lock (_SyncRoot)
            {
                provider = _Provider;
                _Provider = null;
                _Initialized = false;
            }

            if (provider == null)
            {
                return;
            }

            try
            {
                await provider.CleanupDatabaseAsync().ConfigureAwait(false);
            }
            finally
            {
                provider.Dispose();
            }
        }

        #endregion

        #region Private-Methods

        private static TestRuntimeConfiguration LoadDefaultConfiguration()
        {
            TestRuntimeConfiguration configuration = new TestRuntimeConfiguration();

            string? typeValue = Environment.GetEnvironmentVariable("DURABLE_TEST_DB");
            if (!string.IsNullOrWhiteSpace(typeValue) && TryParseDatabaseType(typeValue, out TestDatabaseType parsed))
            {
                configuration.DatabaseType = parsed;
            }

            string? host = Environment.GetEnvironmentVariable("DURABLE_TEST_HOST");
            if (!string.IsNullOrWhiteSpace(host)) configuration.Hostname = host;

            string? port = Environment.GetEnvironmentVariable("DURABLE_TEST_PORT");
            if (!string.IsNullOrWhiteSpace(port) && int.TryParse(port, out int portValue)) configuration.Port = portValue;

            string? user = Environment.GetEnvironmentVariable("DURABLE_TEST_USER");
            if (!string.IsNullOrWhiteSpace(user)) configuration.Username = user;

            string? pass = Environment.GetEnvironmentVariable("DURABLE_TEST_PASS");
            if (!string.IsNullOrWhiteSpace(pass)) configuration.Password = pass;

            string? database = Environment.GetEnvironmentVariable("DURABLE_TEST_DATABASE");
            if (!string.IsNullOrWhiteSpace(database)) configuration.DatabaseName = database;

            string? filename = Environment.GetEnvironmentVariable("DURABLE_TEST_FILE");
            if (!string.IsNullOrWhiteSpace(filename)) configuration.Filename = filename;

            return configuration;
        }

        private static bool TryParseDatabaseType(string value, out TestDatabaseType databaseType)
        {
            switch (value.Trim().ToLowerInvariant())
            {
                case "sqlite":
                    databaseType = TestDatabaseType.Sqlite;
                    return true;
                case "mysql":
                    databaseType = TestDatabaseType.MySql;
                    return true;
                case "postgres":
                case "postgresql":
                case "pgsql":
                    databaseType = TestDatabaseType.Postgres;
                    return true;
                case "sqlserver":
                case "mssql":
                    databaseType = TestDatabaseType.SqlServer;
                    return true;
                default:
                    databaseType = TestDatabaseType.Sqlite;
                    return false;
            }
        }

        #endregion
    }
}
