using System;
using System.Configuration;
using System.Data;
using System.Data.Common;

namespace Dapper
{
    /// <summary>
    /// Extension methods to ease use of ADO.NET Database interaction.
    /// This is not part of the Dapper library, but using the Dapper namespaces means one less using statement.
    /// Written by Jeff Parker 2015
    /// License MIT
    /// </summary>
    public static class DbFactoryExtensions
    {
        static DbFactoryExtensions()
        {
            RegisterOracleProviders();
        }
        
        /// <summary>
        /// Returns a new instance of the provider's class that implements the <see cref="DbConnection"/> class.
        /// </summary>
        /// <param name="config">The configuration information used.</param>
        /// <exception cref="ArgumentNullException">Throws if providerName or connectionString is null or empty.</exception>
        /// <exception cref="NullReferenceException">Throws if no <see cref="DbProviderFactory"/> matching provider name is found.</exception>
        public static DbConnection CreateDbConnection(this ConnectionStringSettings config)
        {
            return CreateDbConnection(config.ProviderName, config.ConnectionString);
        }

        /// <summary>
        /// Returns a <see cref="DataTable"/> that contains information about all installed providers that implement <see cref="DbProviderFactory"/>.
        /// </summary>
        /// <returns></returns>
        public static DataTable GetProviderFactoryClasses()
        {
            return DbProviderFactories.GetFactoryClasses();
        }

        /// <summary>
        /// Returns a new instance of the connection's provider's class that implements the <see cref="DbDataAdapter"/> class.
        /// </summary>
        /// <param name="connection">The connection used.</param>
        /// <returns></returns>
        public static DbDataAdapter CreateDataAdapter(this DbConnection connection)
        {
            return DbProviderFactories.GetFactory(connection).CreateDataAdapter();
        }

        /// <summary>
        /// Returns a new instance of the provider's class that implements the <see cref="DbConnection"/> class.
        /// </summary>
        /// <param name="factory">The provider used.</param>
        /// <param name="connectionString">The connection string used.</param>
        /// <exception cref="NullReferenceException">Throws if the factory could not create the connection.</exception>
        /// <exception cref="ArgumentNullException">Throws if factory or connectionString is null or empty.</exception>
        public static DbConnection CreateConnection(this DbProviderFactory factory, string connectionString)
        {
            if (factory == null) throw new ArgumentNullException("factory");
            if (string.IsNullOrWhiteSpace(connectionString)) throw new ArgumentNullException("connectionString");

            DbConnection connection = factory.CreateConnection();
            if (connection == null) throw new NullReferenceException("Connection could not be created");
            connection.ConnectionString = connectionString;
            return connection;
        }

        /// <summary>
        /// Creates and returns a <see cref="DbConnection"/> object associated with the current connection and command.
        /// </summary>
        /// <param name="connection">The connection used.</param>
        /// <param name="commandText">The command used.</param>
        public static DbCommand CreateCommand(this DbConnection connection, string commandText)
        {
            DbCommand command = connection.CreateCommand();
            command.CommandText = commandText;
            return command;
        }

        /// <summary>
        /// Returns a new instance of the provider's class that implements the <see cref="DbConnection"/> class.
        /// </summary>
        /// <param name="providerName">The provider used.</param>
        /// <param name="connectionString">The connection string used.</param>
        /// <exception cref="ArgumentNullException">Throws if providerName or connectionString is null or empty.</exception>
        /// <exception cref="NullReferenceException">Throws if no <see cref="DbProviderFactory"/> matching provider name is found.</exception>
        public static DbConnection CreateDbConnection(string providerName, string connectionString)
        {
            if (string.IsNullOrWhiteSpace(providerName)) throw new ArgumentNullException("connectionString");
            if (string.IsNullOrWhiteSpace(connectionString)) throw new ArgumentNullException("connectionString");

            var factory = DbProviderFactories.GetFactory(providerName);
            if (factory == null) throw new NullReferenceException("DBProviderFactory could not be found: " + providerName);
            return factory.CreateConnection(connectionString);
        }

        #region Provider Registration

        private static bool _oracleRegistered = false;
        /// <summary>
        /// Registers oracle managed driver <see cref="DbProviderFactory"/>.
        /// </summary>
        public static void RegisterOracleProviders()
        {
            if (_oracleRegistered) return;

            try
            {
                // Register Managed Driver
                RegisterDbProvider("Oracle.ManagedDataAccess.Client", "Oracle Data Provider for .NET, Managed Driver", "ODP.NET, Managed Driver", "Oracle.ManagedDataAccess.Client.OracleClientFactory, Oracle.ManagedDataAccess, Version=4.121.1.0, Culture=neutral, PublicKeyToken=89b483f429c47342");
            }
            catch (Exception ex)
            {
                // log or report exception
            }

            _oracleRegistered = true;
        }

        /// <summary>
        /// Registers a <see cref="DbProviderFactory"/>.
        /// </summary>
        /// <param name="invariant">Class invariant name.</param>
        /// <param name="description">Provider Description.</param>
        /// <param name="name">Provider Name</param>
        /// <param name="type">Provider extended type.</param>
        /// <returns>True if registration suceeded or provider invariant already exists, otherwise false.</returns>
        public static bool RegisterDbProvider(string invariant, string description, string name, string type)
        {
            try
            {
                DataSet ds = ConfigurationManager.GetSection("system.data") as DataSet;
                foreach (DataRow row in ds.Tables[0].Rows)
                {
                    if (row["InvariantName"].ToString() == invariant)
                    {
                        return true;
                    }
                }
                ds.Tables[0].Rows.Add(name, description, invariant, type);
                return true;
            }
            catch { }
            return false;
        }

        #endregion

    }
}