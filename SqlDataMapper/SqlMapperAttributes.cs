using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data.Common;
using System.Reflection;

namespace SqlDataMapper
{
	[Flags]
	public enum SqlMapperFlags
	{
		None,
		Ignore,
		Required,
		NotNull
	}

	interface ISqlMapperAttribute
	{
		SqlMapperFlags Flag { get; set; }
		string Alias { get; set; }
	}

	[AttributeUsage(AttributeTargets.Property, AllowMultiple = false)]
	public class SqlMapperInAttribute : Attribute, ISqlMapperAttribute
	{
		public SqlMapperFlags Flag { get; set; }
		public string Alias { get; set; }

		public SqlMapperInAttribute()
		{
			Flag = SqlMapperFlags.None;
			Alias = String.Empty;
		}
	}

	[AttributeUsage(AttributeTargets.Property, AllowMultiple = false)]
	public class SqlMapperOutAttribute : Attribute, ISqlMapperAttribute
	{
		public SqlMapperFlags Flag { get; set; }
		public string Alias { get; set; }

		public SqlMapperOutAttribute()
		{
			Flag = SqlMapperFlags.None;
			Alias = String.Empty;
		}
	}

	public delegate void SqlMapperDebugCallback(DbDataReader reader);

	[AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
	public class SqlMapperDebugAttribute : Attribute
	{
		public bool Enabled { get; set; }
		public SqlMapperDebugCallback Callback { get; private set; }

		public SqlMapperDebugAttribute(Type type, string method)
		{
			Enabled = true;

			try
			{
				MethodInfo mi = type.GetMethod(method, BindingFlags.Public | BindingFlags.Static | BindingFlags.Instance);
				SqlMapperDebugCallback callback = (SqlMapperDebugCallback)Delegate.CreateDelegate(typeof(SqlMapperDebugCallback), mi);

				Callback = callback;
			}
			catch (Exception ex)
			{
				Callback = null;
				throw new SqlDataMapperException("Can't create callback function.", ex);
			}
		}
	}
}
