// #region Source information
// 
// //*****************************************************************************
// //
// //   table_usr_mstr.cs
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

using DbLinq.Data.Linq;
using DbLinq.Data.Linq.Mapping;

namespace DbLinqSandbox
{
	[Table( Name = "usr_mstr")]
	public class table_usr_mstr
	{
		[Column(IsDbGenerated = true, IsPrimaryKey = true)]
		public int usr_mstr_recid { get; set; }
		
		[Column(IsDbGenerated = true, IsPrimaryKey = true)]
		public string usr_id { get; set; }

		[Column(IsDbGenerated = true, IsPrimaryKey = true)]
		public string usr_mail  { get; set; }

		[Column]
		public string usr_passwd  { get; set; }

		[Column]
		public bool usr_active { get; set; }

		[Column]
		public string usr_title { get; set; }

		[Column]
		public string usr_first_name { get; set; }

		[Column]
		public string usr_last_name { get; set; }

		[Column]
		public string usr_db_name  { get; set; }

		[Column]
		public string usr_db_host { get; set; }

		[Column]
		public string usr_addr_zip { get; set; }

		[Column]
		public string usr_addr_country { get; set; }

		[Column]
		public string usr_addr_state { get; set; }

		[Column]
		public string usr_addr_city { get; set; }

		[Column]
		public string usr_addr_addr1 { get; set; }

		[Column]
		public string usr_addr_addr2 { get; set; }

		[Column]
		public string usr_phone1 { get; set; }

		[Column]
		public string usr_phone2  { get; set; }

		[Column]
		public string usr_cell1 { get; set; }

		[Column]
		public string usr_cell2 { get; set; }

		[Column]
		public string usr_fax1 { get; set; }

		[Column]
		public string usr_fax2 { get; set; }

		[Column]
		public string usr_note  { get; set; }

	}
}

