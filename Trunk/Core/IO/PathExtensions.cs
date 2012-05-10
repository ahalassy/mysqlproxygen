#region Source information

//*****************************************************************************
//
//   PathExtensions.cs
//   Created by Adam Halassy (2012.05.10. 0:00:00)
//
// ---------------------------------------------------------------------------
//
//   Copyright Adam Halassy, Budapest, HUN.
//   All rights reserved worldwide.  This is an unpublished work.
//
//*****************************************************************************

#endregion

using System;

namespace System.IO
{
	public static class PathExtensions
	{
		public static string NormalizePath(string path)
		{
			return (path ?? "").Replace('\\', Path.DirectorySeparatorChar);
		}
	}
}

