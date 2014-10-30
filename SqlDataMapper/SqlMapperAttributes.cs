using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data.Common;
using System.Reflection;

namespace SqlDataMapper
{
	/// <summary>
	/// Flag for in and out options.
	/// </summary>
	[Flags]
	public enum SqlMapperFlags
	{
		/// <summary>
		/// No flags.
		/// </summary>
		None,

		/// <summary>
		/// Ignore this column or member.
		/// </summary>
		Ignore,

		/// <summary>
		/// The column must exists in result set.
		/// </summary>
		Required,

		/// <summary>
		/// The column must exists in result set and can't null.
		/// </summary>
		NotNull
	}

	interface ISqlMapperAttribute
	{
		SqlMapperFlags Flag { get; set; }
		string Alias { get; set; }
	}

	/// <summary>
	/// Set options for database-to-class.
	/// </summary>
	[AttributeUsage(AttributeTargets.Property, AllowMultiple = false)]
	public class SqlMapperInAttribute : Attribute, ISqlMapperAttribute
	{
		/// <summary>
		/// Flag.
		/// </summary>
		public SqlMapperFlags Flag { get; set; }
		
		/// <summary>
		/// The alias name for the column.
		/// </summary>
		public string Alias { get; set; }

		/// <summary>
		/// Set in options.
		/// </summary>
		public SqlMapperInAttribute()
		{
			Flag = SqlMapperFlags.None;
			Alias = String.Empty;
		}
	}

	/// <summary>
	/// Set options class-to-database.
	/// </summary>
	[AttributeUsage(AttributeTargets.Property, AllowMultiple = false)]
	public class SqlMapperOutAttribute : Attribute, ISqlMapperAttribute
	{
		/// <summary>
		/// Flag.
		/// </summary>
		public SqlMapperFlags Flag { get; set; }

		/// <summary>
		/// The alias name for the column.
		/// </summary>
		public string Alias { get; set; }

		/// <summary>
		/// Set out options.
		/// </summary>
		public SqlMapperOutAttribute()
		{
			Flag = SqlMapperFlags.None;
			Alias = String.Empty;
		}
	}

	/// <summary>
	/// A callback to provide a debug method.
	/// </summary>
	/// <param name="reader">The DbDataReader object.</param>
	public delegate void SqlMapperDebugCallback(DbDataReader reader);

	/// <summary>
	/// Set options to debug a class.
	/// </summary>
	[AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
	public class SqlMapperDebugAttribute : Attribute
	{
		/// <summary>
		/// Enable or disable the debug.
		/// </summary>
		public bool Enabled { get; set; }

		/// <summary>
		/// The callback to debug the class.
		/// </summary>
		public SqlMapperDebugCallback Callback { get; private set; }

		/// <summary>
		/// Debug a class. Disabled state.
		/// </summary>
		internal SqlMapperDebugAttribute()
		{
			Enabled = false;
		}

		/// <summary>
		/// Debug a class.
		/// </summary>
		/// <param name="type">The type of the source class.</param>
		/// <param name="method">The method name of the source class.</param>
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
