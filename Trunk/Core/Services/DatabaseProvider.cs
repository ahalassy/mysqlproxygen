#region Source information
 
//*****************************************************************************
//
//   DatabaseProvider.cs
//   Created by Adam Halassy <adam.halassy@gmail.com> (2012.05.01. 0:00:00)
//
// ---------------------------------------------------------------------------
//
//    Copyright (c) 2012 Adam Halassy
//   All rights reserved worldwide. Licensed by the terms of GPLv3
//
//*****************************************************************************
 
#endregion

using System;
using System.Collections.Generic;

using MySql.Data.MySqlClient;

namespace MySqlDevTools.Services
{
    public class DatabaseProvider: IDisposable
    {
        public bool _disposed = false;
        
        private MySqlConnection _connection = null;
        
        public bool Disposed { get { return _disposed; } }
        
        public MySqlConnection Connection
        { 
            get
            { 
                CheckObject();
                return _connection;
            }
        }
        
        private void CheckObject()
        {
            if (Disposed)
                throw new ObjectDisposedException (
                    String.Format("{0} instance already disposed!", this.GetType().FullName)
                    );
        }
        
        public MySqlCommand GetCommand(string sql)
        {
            CheckObject();
            
            return new MySqlCommand (
                sql,
                this.Connection
                );
        }
            
        public string[] QueryTables()
        {
            CheckObject();
            MySqlCommand command = GetCommand("show tables;");
            using ( MySqlDataReader reader = command.ExecuteReader())
            {
                List<string> result = new List<string> ();
                while (reader.Read())
                    result.Add(reader.GetString(0));

                return result.ToArray();
            }
        }
        
        public DatabaseProvider (string connectionString)
        {
            _connection = new MySqlConnection (connectionString);
            _connection.Open();
        }

        #region IDisposable implementation
        public void Dispose()
        {
            CheckObject();
            
            try
            {
                Connection.Close();
            }
            catch
            {
                
            }
            finally
            {
                _disposed = true;
            }
        }
        #endregion
    }
}

