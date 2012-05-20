// #region Source information
// 
// //*****************************************************************************
// //
// //   CsSchemaDatabase.cs
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
using System.Data.Linq;
using System.Data.Linq.Mapping;
using System.Reflection;

using DbLinq.Data.Linq;
using DbLinq.Data.Linq.Mapping;

using MySql.Data.MySqlClient;

using DataContext = DbLinq.MySql.MySqlDataContext;

namespace DbLinqSandbox
{
	[Database(Name = "cs_schema")]
	public class CsSchemaDatabase: DataContext
	{
		public DbLinq.Data.Linq.Table<table_usr_mstr> usr_mstr		{ get { return base.GetTable<table_usr_mstr>(); } }
		
		public CsSchemaDatabase (MySqlConnection connection):
			base (connection)
		{
		}
	}
}

