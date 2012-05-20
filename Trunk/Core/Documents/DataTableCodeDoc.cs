// #region Source information
// 
// //*****************************************************************************
// //
// //   DataTableCodeDoc.cs
// //   Created by Adam Halassy <adam.halassy@gmail.com> (2012.05.01. 0:00:00)
// //
// // ---------------------------------------------------------------------------
// //
// //    Copyright (c) 2012 Adam Halassy
// //   All rights reserved worldwide. Document licensed by the terms of GPLv3
// //
// //*****************************************************************************
// 
// #endregion
// 
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;

using MySqlDevTools.Documents;

namespace MySqlDevTools
{
	public class DataTableCodeDoc
	{
		private string
			_tableName;
		private List<FieldInformation> _fields = new List<FieldInformation> ();
		
		public string TableName { get { return _tableName; } }
		
		public FieldInformation[] Fields { get { return _fields.ToArray(); } }
		
		private void FetchFields(DataTable fields)
		{
			foreach (DataRow row in fields.Rows)
				_fields.Add(
					new FieldInformation (
						row [0] as string, 
						row [1] as string,
						(row [3] as string ?? "").ToUpper() == "PRI"
						)
					);
		}
		
		public DataTableCodeDoc (string tableName, DataTable fields)
		{
			_tableName = tableName;
			FetchFields(fields);
		}
	}
}

