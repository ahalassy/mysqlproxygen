#region Source information

//*****************************************************************************
//
//   DbProxyClass.cs
//   Created by ahalassy (2012.04.15. 0:00:00)
//
// ---------------------------------------------------------------------------
//
//   Copyright Adam Halassy, Budapest, HUN.
//   All rights reserved worldwide. Document licensed by the terms of GPLv3
//
//*****************************************************************************
// D:\Repo\MySqlProxyGen.Net\Trunk\Halassy.DbProxy\Data\
//*****************************************************************************

#endregion

using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading;
using System.Reflection;

namespace Halassy.Data
{
    public class DbProxyClass : IDisposable
    {		
        private bool
			_externalConnection = false,
            _busy = false,
            _opened = false,
            _disposed = false;

        private DbConnection 
			_connection = null;

        public DbManagementObjectFactoryClass MgmtObjectFactory { get; private set; }

        protected AutoResetEvent _connectionReleaseSemaphore = new AutoResetEvent(false);

        protected DbConnection CurrentConnection { get { return _connection; } }
		
		public bool ExternalConnection {get { return _externalConnection; } }
		
		/// <summary>
		/// Gets a value indicating whether this <see cref="Halassy.Data.DbProxyClass"/> connection busy.
		/// </summary>
		/// <value>
		/// <c>true</c> if connection busy; otherwise, <c>false</c>.
		/// </value>
        public bool ConnectionBusy { get { return _busy; } }
		
		/// <summary>
		/// Gets a value indicating whether this <see cref="Halassy.Data.DbProxyClass"/> is opened.
		/// </summary>
		/// <value>
		/// <c>true</c> if opened; otherwise, <c>false</c>.
		/// </value>
        public bool Opened { get { return _opened; } }
		
		/// <summary>
		/// Gets or sets the connection string.
		/// </summary>
		/// <value>
		/// The connection string.
		/// </value>
        public string ConnectionString { get; private set; }

        private object InternalExecuteSqlCommand(string sql, CommandExecution mode)
        {
            DbCommand command = MgmtObjectFactory.CreateCommand(sql, CurrentConnection);
            return InternalExecuteCommand(command, mode);
        }

        private object InternalExecuteCommand(DbCommand command, CommandExecution mode)
        {
            try
            {
                LockConnection();

                switch (mode)
                {
                    case CommandExecution.Query:
                        return command.ExecuteReader();

                    case CommandExecution.Scalar:
                        return command.ExecuteScalar();

                    case CommandExecution.Command:
                        command.ExecuteNonQuery();
                        return null;

                    default: return null;
                }

            }
            finally
            {
                ReleaseConnection();
            }
        }

        /// <summary>
        /// Locks the actual connection
        /// </summary>
        protected void LockConnection()
        {
            while (ConnectionBusy)
                _connectionReleaseSemaphore.WaitOne();
            _busy = true;
        }

        /// <summary>
        /// Releases the actual connection
        /// </summary>
        protected void ReleaseConnection()
        {
            if (!ConnectionBusy)
                return;

            _busy = false;
            _connectionReleaseSemaphore.Set();
        }
		
		/// <summary>
		/// Executes the sql query and returns the result as a datatable
		/// </summary>
		/// <returns>
		/// Queried data as a datatable
		/// </returns>
		/// <param name='sql'>
		/// Sql statement
		/// </param>
        public DataTable ExecuteSqlQuery(string sql)
        {
            DbDataReader reader = InternalExecuteSqlCommand(sql, CommandExecution.Query) as DbDataReader;
            DataTable result = new DataTable();

            if (reader != null)
            {
                result.Load(reader);
                reader.Close();
            }

            return result;
        }
		
		/// <summary>
		/// Executes the SQL statement, and returns a scalar value as result
		/// </summary>
		/// <returns>
		/// The return value as a scalar type
		/// </returns>
		/// <param name='sql'>
		/// Sql statement
		/// </param>
        public object ExecuteSqlScalar(string sql)
        {
            return InternalExecuteSqlCommand(sql, CommandExecution.Scalar);
        }
		
		/// <summary>
		/// Executes the SQL statement and returns nothing
		/// </summary>
		/// <param name='sql'>
		/// SQL statement
		/// </param>
        public void ExecuteSqlCommand(string sql)
        {
            InternalExecuteSqlCommand(sql, CommandExecution.Command);
        }
		
		/// <summary>
		/// Runs the specified stored routine
		/// </summary>
		/// <returns>The procedure's return datatable</returns>
		/// <param name='name'>Routine name</param>
		/// <param name='parms'>Routine parameters</param>
        public DataTable RunProcedure(string name, DbStoredRoutineParmCollection parms)
        {
            DbCommand command = MgmtObjectFactory.CreateCommand(
                name,
                CurrentConnection
                );

            command.CommandType = CommandType.StoredProcedure;
            parms.FillParameters(command);
            DbDataReader reader = InternalExecuteCommand(command, CommandExecution.Query) as DbDataReader;
            DataTable result = new DataTable();
            if (reader != null)
            {
                result.Load(reader);
                reader.Close();
            }
            parms.FetchParameters(command);

            return result;
        }
	
		/// <summary>
		/// Calls the specified stored function.
		/// </summary>
		/// <param name='name'>Stored function's name</param>
		/// <param name='parms'>Routine parameters</param>
        public void CallFunction(string name, DbStoredRoutineParmCollection parms)
        {
            DbCommand command = MgmtObjectFactory.CreateCommand(
                name,
                CurrentConnection
                );

            parms.FillParameters(command);
            InternalExecuteCommand(command, CommandExecution.Scalar);
            parms.FetchParameters(command);

        }

		/// <summary>
		/// Opens the connection.
		/// </summary>
		/// <exception cref='InvalidOperationException'>
		/// Is thrown when an operation cannot be performed.
		/// </exception>
        public void OpenConnection()
        {
            if (Opened)
                throw new InvalidOperationException("Database connection already opened!");
			
			if (ExternalConnection)
				_connection.Open ();
			else
			{
	            _connection = MgmtObjectFactory.CreateConnection(ConnectionString);
	            _connection.Open();
	            _opened = true;
				
			}
        }
		
        /// <summary>
        /// Closes the connection.
        /// </summary>
        /// <exception cref='InvalidOperationException'>
        /// Is thrown when an operation cannot be performed.
        /// </exception>
		public void CloseConnection()
        {
            if (!Opened)
                throw new InvalidOperationException("There's no opened database connection!");
            try
            {
				if (ExternalConnection)
					_connection.Close ();
				else
				{
	                _connection.Close();
	                _connection.Dispose();
	                _connection = null;					
				}
            }
            finally
            {
                _opened = false;
            }
        }
		
		/// <summary>
		/// Initializes a new instance of the <see cref="Halassy.Data.DbProxyClass"/> class.
		/// </summary>
		/// <param name='connectionString'>
		/// Connection string.
		/// </param>
		/// <param name='mgmtObjFactoryType'>
		/// Object factory type to build the required database specific objects
		/// </param>
		/// <exception cref='InvalidCastException'>
		/// Is thrown when an explicit conversion (casting operation) fails because the source type cannot be converted to the
		/// destination type.
		/// </exception>
        public DbProxyClass(string connectionString, Type mgmtObjFactoryType)
        {
            if (!typeof(DbManagementObjectFactoryClass).IsAssignableFrom(mgmtObjFactoryType))
                throw new InvalidCastException(
                    String.Format(
                        "The shipped factory class type must be inherited from {0}!",
                        typeof(DbManagementObjectFactoryClass).FullName
                        )
                    );

            this.ConnectionString = connectionString;

            ConstructorInfo constructor = mgmtObjFactoryType.GetConstructor(
                new Type[] { typeof(DbProxyClass) }
                );
            this.MgmtObjectFactory = constructor.Invoke(new object[] { this }) as DbManagementObjectFactoryClass;
        }
		
		/// <summary>
		/// Initializes a new instance of the <see cref="Halassy.Data.DbProxyClass"/> class.
		/// </summary>
		/// <param name='connection'>
		/// Connection.
		/// </param>
		/// <param name='mgmtObjFactoryType'>
		/// Mgmt object factory type.
		/// </param>
		/// <exception cref='InvalidCastException'>
		/// Is thrown when an explicit conversion (casting operation) fails because the source type cannot be converted to the
		/// destination type.
		/// </exception>
		public DbProxyClass(DbConnection connection, Type mgmtObjFactoryType)
		{
			if (!typeof(DbManagementObjectFactoryClass).IsAssignableFrom(mgmtObjFactoryType))
                throw new InvalidCastException(
                    String.Format(
                        "The shipped factory class type must be inherited from {0}!",
                        typeof(DbManagementObjectFactoryClass).FullName
                        )
                    );
			
			_connection = connection;
			_externalConnection = true;
			_opened = connection.State != ConnectionState.Broken && connection.State != ConnectionState.Closed;
		}
		
		/// <summary>
		/// Releases unmanaged resources and performs other cleanup operations before the
		/// <see cref="Halassy.Data.DbProxyClass"/> is reclaimed by garbage collection.
		/// </summary>
        ~DbProxyClass()
        {
            if (!_disposed)
                Dispose();
        }

        #region IDisposable Members
		/// <summary>
		/// Releases all resource used by the <see cref="Halassy.Data.DbProxyClass"/> object.
		/// </summary>
		/// <remarks>
		/// Call <see cref="Dispose"/> when you are finished using the <see cref="Halassy.Data.DbProxyClass"/>. The
		/// <see cref="Dispose"/> method leaves the <see cref="Halassy.Data.DbProxyClass"/> in an unusable state. After
		/// calling <see cref="Dispose"/>, you must release all references to the <see cref="Halassy.Data.DbProxyClass"/> so
		/// the garbage collector can reclaim the memory that the <see cref="Halassy.Data.DbProxyClass"/> was occupying.
		/// </remarks>
        public void Dispose()
        {
            try
            {
                try
                {
                    if (_connection.State != ConnectionState.Closed || _connection.State != ConnectionState.Broken)
                        _connection.Close();

                    _connection.Dispose();
                }
                catch { }

            }
            finally
            {
                _disposed = true;
            }
        }

        #endregion
    }
}
