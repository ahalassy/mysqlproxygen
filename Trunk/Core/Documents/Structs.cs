#region Source information

//*****************************************************************************
//
//   Structs.cs
//   Created by Adam Halassy (2012.03.26. 0:00:00)
//
// ---------------------------------------------------------------------------
//
//   Copyright Adam Halassy, Budapest, HUN.
//   All rights reserved worldwide.  This is an unpublished work.
//
//*****************************************************************************

#endregion
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;

namespace MySqlDevTools.Documents
{
    #region internal struct DefaultTypeValueHolder
	internal struct DefaultTypeValueHolder
	{
		public static DefaultTypeValueHolder Blank
		{
			get
			{
				DefaultTypeValueHolder result = new DefaultTypeValueHolder ();
				result._blank = true;

				return result;
			}
		}

		private bool
            _blank;
		private Type
            _clrType;
		private string
            _csharpCode;

		public bool IsBlank { get { return _blank; } }

		public Type ClrType { get { return IsBlank ? null : _clrType; } }

		public string CSharpCode { get { return IsBlank ? null : _csharpCode; } }

		public DefaultTypeValueHolder (Type type, string csharpCode)
		{
			_blank = false;
			_clrType = type;
			_csharpCode = csharpCode;
		}
	}

    #endregion

    #region internal struct RoutineParameterMapping
	internal struct RoutineParameterMapping
	{
		private string
            _typeName;
		private bool
			_nullable;
		private Type
            _clrType;

		public string SqlTypeName { get { return _typeName; } }

		public Type ClrType { get { return _clrType; } }
		
		public bool Nullable { get { return _nullable; } }
		
		public bool IsMatch(string sqlType)
		{
			return SqlTypeName.ToUpper() == sqlType.ToUpper();
		}
	
		public RoutineParameterMapping (string sqlType, Type clrType, bool nullable)
		{
			_nullable = nullable;
			_clrType = clrType;
			_typeName = sqlType;
		}
	}

    #endregion

	#region public struct FieldInformation
	public struct FieldInformation
	{
		private bool
			_isKey;
		private string
			_sqlType,
			_fieldName;
		
		public bool IsKey { get { return _isKey; } }
		
		public string FieldName { get { return _fieldName; } }
		
		public string SqlType { get { return _sqlType; } }
		
		public string ClrTypeName
		{
			get	{ return RoutineParameter.GetClrTypeName(SqlType, true); }
		}
		
		public string CSharpCode
		{
			get
			{
				return String.Format(
					"public {0} {1} {{ get; set; }}",
					ClrTypeName,
					FieldName
					);
			}
		}
		
		public FieldInformation (string fld, string sqlType, bool key)
		{
			_fieldName = fld;
			_sqlType = (sqlType ?? "");
			_isKey = key;
			
			if (_sqlType.Contains('('))
				_sqlType = _sqlType.Substring(0, _sqlType.IndexOf('('));
		}
	}
	
	#endregion
	
    #region public struct RoutineParameter
	public struct RoutineParameter
	{
		internal static readonly RoutineParameterMapping[] SqlTypeNameMapping = new RoutineParameterMapping[]{
            new RoutineParameterMapping ("CHAR", typeof(char), false),
            new RoutineParameterMapping ("VARCHAR", typeof(string), true),
            new RoutineParameterMapping ("TINYTEXT", typeof(string), true),
            new RoutineParameterMapping ("TEXT", typeof(string), true),
            new RoutineParameterMapping ("BLOB", typeof(byte[]), true),
            new RoutineParameterMapping ("MEDIUMTEXT", typeof(string), true),
            new RoutineParameterMapping ("MEDIUMBLOB", typeof(byte[]), true),
            new RoutineParameterMapping ("LONGTEXT", typeof(string), true),
            new RoutineParameterMapping ("LONGBLOB", typeof(string), true),
            new RoutineParameterMapping ("TINYINT", typeof(int), false),
            new RoutineParameterMapping ("SMALLINT", typeof(int), false),
            new RoutineParameterMapping ("MEDIUMINT", typeof(int), false),
            new RoutineParameterMapping ("INT", typeof(int), false),
            new RoutineParameterMapping ("BIGINT", typeof(long), false),
            new RoutineParameterMapping ("FLOAT", typeof(float), false),
            new RoutineParameterMapping ("DOUBLE", typeof(double), false),
            new RoutineParameterMapping ("DECIMAL", typeof(decimal), false),
			new RoutineParameterMapping ("BIT", typeof(bool), false),
            new RoutineParameterMapping ("DATE", typeof(DateTime), false),
            new RoutineParameterMapping ("DATETIME", typeof(DateTime), false),
            new RoutineParameterMapping ("TIMESTAMP", typeof(DateTime), false),
            new RoutineParameterMapping ("TIME", typeof(DateTime), false)
            };
		internal static DefaultTypeValueHolder[] NonNullableTypes = new DefaultTypeValueHolder[]{
            new DefaultTypeValueHolder (typeof(char), "\'\\0\'"),
            new DefaultTypeValueHolder (typeof(int), "0"),
            new DefaultTypeValueHolder (typeof(long), "0"),
            new DefaultTypeValueHolder (typeof(float), "0"),
            new DefaultTypeValueHolder (typeof(double), "0"),
            new DefaultTypeValueHolder (typeof(decimal), "0"),
            new DefaultTypeValueHolder (typeof(DateTime), "DateTime.MinValue")
            };
		
		private static bool IsDirection(string tag)
		{
			tag = (tag ?? "").ToUpper();
			return (tag == "IN" || tag == "OUT");
		}
		
		private static string CutTag(ref string text)
		{
			int pos = text.IndexOf(' ');

			string result = pos < 0 ? text : text.Substring(0, pos);
			text = pos < 0 ? "" : text.Substring(pos + 1);

			text = text.Trim();
			return result.Trim();
		}

		public static ParameterDirection ParseDirection(string direction)
		{
			if (direction.ToUpper() == "IN")
				return ParameterDirection.Input;

			if (direction.ToUpper() == "OUT")
				return ParameterDirection.Output;

			if (direction.ToUpper() == "INOUT")
				return ParameterDirection.InputOutput;

			return ParameterDirection.ReturnValue;
		}
		
		public static string GetClrTypeName(string sqlType)
		{
			return GetClrTypeName(sqlType, false);
		}
		
		public static string GetClrTypeName(string sqlType, bool nullable)
		{
			foreach (RoutineParameterMapping mapping in SqlTypeNameMapping)
				if (mapping.IsMatch(sqlType))
					return (!mapping.Nullable && nullable) ?
						mapping.ClrType.FullName + "?" :
						mapping.ClrType.FullName;

			return typeof(object).FullName;
		}

		private bool
            _unsigned;
		private string
            _name,
            _sqlDirection,
            _sqlType;

		public bool IsOutGoing
		{
			get
			{
				return ParameterDirection == ParameterDirection.InputOutput
                    || ParameterDirection == ParameterDirection.Output;
			}
		}

		public string Name { get { return _name; } }

		public string SqlDirection { get { return _sqlDirection; } }

		public string SqlType { get { return _sqlType; } }

		public bool Unsigned { get { return _unsigned; } }

		public ParameterDirection ParameterDirection { get { return ParseDirection(SqlDirection); } }

		public string ClrTypeName { get { return GetClrTypeName(this.SqlType); } }

		internal DefaultTypeValueHolder GetClrNullValue()
		{

			RoutineParameterMapping? typeMapping = null;
			foreach (RoutineParameterMapping mapping in SqlTypeNameMapping)
				if (mapping.IsMatch(this.SqlType))
				{
					typeMapping = mapping;
					break;
				}

			if (typeMapping == null)
				return DefaultTypeValueHolder.Blank;
			else
			{
				foreach (DefaultTypeValueHolder holder in NonNullableTypes)
					if (holder.ClrType.Equals(typeMapping.Value.ClrType))
						return holder;

				return DefaultTypeValueHolder.Blank;
			}
		}

		public string CSharpCode
		{
			get
			{
				return String.Format(
                    ParameterDirection == ParameterDirection.Input ?
                        "{1} {2}" : "{0} {1} {2}",
                        ParameterDirection == ParameterDirection.InputOutput ? "ref" : "out",
                        GetClrTypeName(SqlType),
                        Name
                        );

			}
		}

		public override string ToString()
		{
			return String.Format("{0} {1} as {2}", SqlDirection, Name, SqlType);
		}

		public RoutineParameter (string sqlParmCode)
		{
			sqlParmCode = sqlParmCode.Trim().Replace('\t', ' ');

			_unsigned = sqlParmCode.ToUpper().Contains("UNSIGNED");

			_sqlDirection = CutTag(ref sqlParmCode);
            
			if (IsDirection(_sqlDirection))
				_name = CutTag(ref sqlParmCode);
			else
			{
				_name = _sqlDirection;
				_sqlDirection = "IN";
			}
			
			_sqlType = sqlParmCode.Trim().ToUpper().Replace("UNSIGNED", "");

			if (_sqlType.Contains('('))
				_sqlType = _sqlType.Substring(0, _sqlType.IndexOf('('));
		}

	}

    #endregion

    #region public struct MySqlMacro
	public struct MySqlMacro
	{
		private string
            _content,
            _name;

		public string Name { get { return _name; } }

		public string Content { get { return _content; } }

		public override string ToString()
		{
			return String.Format("{0} = \"{1}\"", Name, Content);
		}

		public MySqlMacro (string name, string content)
		{
			_name = name;
			_content = content;
		}
	}

    #endregion
}
