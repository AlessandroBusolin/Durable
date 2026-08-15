namespace Test.Shared
{
    /// <summary>
    /// Enumerates the database providers supported by the Durable test suites.
    /// </summary>
    public enum TestDatabaseType
    {
        /// <summary>
        /// SQLite provider. Runs fully in-process and requires no external server.
        /// </summary>
        Sqlite,

        /// <summary>
        /// MySQL provider. Requires a reachable MySQL server (or a dockerized instance).
        /// </summary>
        MySql,

        /// <summary>
        /// PostgreSQL provider. Requires a reachable PostgreSQL server (or a dockerized instance).
        /// </summary>
        Postgres,

        /// <summary>
        /// Microsoft SQL Server provider. Requires a reachable SQL Server instance (or a dockerized instance).
        /// </summary>
        SqlServer
    }
}
