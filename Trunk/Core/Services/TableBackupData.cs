#region Source information

//*****************************************************************************
//
//   TableBackupData.cs
//   Created by ahalassy (2013.04.27. 0:00:00)
//
// ---------------------------------------------------------------------------
//
//   Copyright Adam Halassy, Budapest, HUN.
//   All rights reserved worldwide.  This is an unpublished work.
//
//*****************************************************************************
// D:\Repo\MySqlProxyGen.Net\Trunk\Core\Services\
//*****************************************************************************

#endregion

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MySql.Data.MySqlClient;
using System.Data;

namespace MySqlDevTools.Services
{
    public class TableBackupData
    {
        public string TableName { get; private set; }

        public string CreateCommand { get; private set; }

        public bool ValidTable { get { return !String.IsNullOrEmpty(CreateCommand); } }

        public DataTable Content { get; private set; }

        public string CreateCommandQuery
        {
            get
            {
                return String.Format("SHOW CREATE TABLE {0};", TableName);
            }
        }

        public string QueryContentCommand
        {
            get { return String.Format("SELECT * FROM {0};", TableName); }
        }

        public string CheckTableCommand
        {
            get { return String.Format("SHOW TABLES LIKE '{0}';", TableName); }
        }

        public string QueryFieldsCommand
        {
            get { return String.Format("SHOW FIELDS FROM {0};", TableName); }
        }

        private void InsertContent(MySqlConnection connection, string[] fields)
        {
            string fieldList = "";
            foreach (string field in fields)
                fieldList += (String.IsNullOrEmpty(fieldList) ? "" : ", ")
                    + field;
            string query = String.Format("SELECT {0} FROM {1}", fieldList, TableName);

            using (MySqlDataAdapter adapter = new MySqlDataAdapter(query, connection))
            {
                DataSet dataSet = new DataSet("BackupDataSet");
                new MySqlCommandBuilder(adapter);

                adapter.Fill(dataSet, TableName);

                DataTable table = dataSet.Tables[TableName];
                foreach (DataRow src in Content.Rows)
                {
                    DataRow target = table.NewRow();
                    foreach (string fieldName in fields)
                        target[fieldName] = src[fieldName];
                    table.Rows.Add(target);
                }

                adapter.Update(dataSet, TableName);
            }
        }

        private IEnumerable<string> QueryFields(MySqlConnection connection)
        {
            MySqlCommand command = new MySqlCommand(QueryFieldsCommand, connection);
            using (MySqlDataReader reader = command.ExecuteReader())
            {
                while (reader.Read())
                    yield return reader.GetString("Field");
            }
        }

        private void RestoreSchema(MySqlConnection connection)
        {
            MySqlCommand command = new MySqlCommand(CreateCommand, connection);
            command.ExecuteNonQuery();
        }

        private void ObtainCreateCommand(MySqlConnection connection)
        {
            try
            {
                using (MySqlCommand command = new MySqlCommand(CreateCommandQuery, connection))
                {
                    using (MySqlDataReader reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            CreateCommand = reader.GetString(1);
                            return;
                        }
                    }
                }
            }
            finally
            {
                if (!ValidTable) throw new Exception(String.Format("Failed to backup {0}!", TableName));
            }
        }

        public void BackupContent(MySqlConnection connection)
        {
            ObtainCreateCommand(connection);

            MySqlCommand command = new MySqlCommand(QueryContentCommand, connection);
            using (MySqlDataReader reader = command.ExecuteReader())
            {
                Content = new DataTable();
                Content.Load(reader);
            }
        }

        public void RestoreContent(MySqlConnection connection)
        {
            /* 1. Megnézzük, hogy megvan-e még a tábla. Ha nincs, akkor visszaállítjuk a 
             *  CreateCommand szerint.
             *  
             * 2. Lekérdezzük a tábla mezőit. Azokból a mezőkből, amik itt is ott is megvannak
             *  csinálunk egy insert into parancsot, majd minden sorra nekiindítjuk
             */
            if (!IsTableExists(connection))
                RestoreSchema(connection);

            List<string> fieldList = new List<string>();
            foreach (string field in QueryFields(connection))
                if (Content.Columns.Contains(field))
                    fieldList.Add(field);

            InsertContent(connection, fieldList.ToArray());
        }

        public bool IsTableExists(MySqlConnection connection)
        {
            MySqlCommand command = new MySqlCommand(CheckTableCommand, connection);
            using (MySqlDataReader reader = command.ExecuteReader())
                return reader.HasRows;
        }

        public TableBackupData(string tableName)
        {
            TableName = tableName;
        }

        internal void Clear()
        {
            if (Content != null)
                Content.Clear();

            Content = null;
            TableName = null;
        }
    }
}
