#region Source information
 
//*****************************************************************************
//
//   DbProxyException.cs
//   Created by Adam Halassy <adam.halassy@gmail.com> (2012.05.01. 0:00:00)
//
// ---------------------------------------------------------------------------
//
//    Copyright (c) 2012 Adam Halassy
//   All rights reserved worldwide. This is an unpublished work!
//
//*****************************************************************************

#endregion


using System;

namespace Halassy.Data
{
	public class DbProxyException: Exception
	{
		public DbStoredRoutineParmCollection Parameters { get; private set; }
		
		public string RoutineName { get; private set; }
		
		public DbProxyException (string routineName, DbStoredRoutineParmCollection parms, Exception ex )
			:base(String.Format("Error calling \"{0}\"! Message was \"{1}\".", routineName, ex.Message), ex)
		{
			this.Parameters = parms;
			this.RoutineName = routineName;
		}
	}
}

