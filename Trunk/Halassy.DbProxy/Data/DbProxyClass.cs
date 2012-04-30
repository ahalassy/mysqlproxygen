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

namespace Halassy.Data
{
    public class DbProxyClass : IDisposable
    {
        private bool
            _busy = false,
            _opened = false,
            _disposed = false;

        private DbConnection _connection = null;

        public DbManagementObjectFactoryClass MgmtObjectFactory { get; private set; }

        protected AutoResetEvent _connectionReleaseSemaphore = new AutoResetEvent(false);

        protected DbConnection CurrentConnection { get { return _connection; } }

        public bool ConnectionBusy { get { return _busy; } }

        public bool Opened { get { return _opened; } }

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

        public object ExecuteSqlScalar(string sql)
        {
            return InternalExecuteSqlCommand(sql, CommandExecution.Scalar);
        }

        public void ExecuteSqlCommand(string sql)
        {
            InternalExecuteSqlCommand(sql, CommandExecution.Command);
        }

        public DataTable RunProcedure(string name, DbStoredRoutineParms parms)
        {
            DbCommand command = MgmtObjectFactory.CreateCommand(
                name,
                CurrentConnection
                );

            parms.FillParameters(command);
            DbDataReader reader = InternalExecuteCommand(command, CommandExecution.Query) as DbDataReader;
            parms.FetchParameters(command);
            DataTable result = new DataTable();
            if (reader != null)
            {
                result.Load(reader);
                reader.Close();
            }
            return result;
        }

        public void CallFunction(string name, DbStoredRoutineParms parms)
        {
            DbCommand command = MgmtObjectFactory.CreateCommand(
                name,
                CurrentConnection
                );

            parms.FillParameters(command);
            InternalExecuteCommand(command, CommandExecution.Scalar);
            parms.FetchParameters(command);

        }

        public void OpenConnection()
        {
            if (Opened)
                throw new InvalidOperationException("Database connection already opened!");

            _connection = MgmtObjectFactory.CreateConnection(ConnectionString);
            _connection.Open();
            _opened = true;
        }

        public void CloseConnection()
        {
            if (!Opened)
                throw new InvalidOperationException("There's no opened database connection!");
            try
            {
                _connection.Close();
                _connection.Dispose();
                _connection = null;
            }
            finally
            {
                _opened = false;
            }
        }

        public DbProxyClass(string connectionString, DbManagementObjectFactoryClass mgmtObjFactory)
        {
            this.MgmtObjectFactory = mgmtObjFactory;
            this.ConnectionString = connectionString;
        }

        ~DbProxyClass()
        {
            if (!_disposed)
                Dispose();
        }

        #region IDisposable Members

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
