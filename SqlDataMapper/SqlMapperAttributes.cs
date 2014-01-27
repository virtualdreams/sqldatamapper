using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data.Common;
using System.Reflection;

namespace SqlDataMapper
{
	/// <summary>
	/// Flags for property attributes.
	/// </summary>
	[Flags]
	public enum SqlMapperProperty
	{
		/// <summary>
		/// No function
		/// </summary>
		None = 0x0,
		
		/// <summary>
		/// Ignore this member
		/// </summary>
		Ignored = 0x1,
		
		/// <summary>
		/// This member is required
		/// </summary>
		Required = 0x2,
		
		/// <summary>
		/// This member is required and can't null
		/// </summary>
		NotNull = 0x4
	}
	
	/// <summary>
	/// Provides an attribute for properties to enhance the core mapper.
	/// </summary>
	[AttributeUsage(AttributeTargets.Property, AllowMultiple = false)]
	public class SqlMapperAttributes: Attribute
	{
	
		/// <summary>
		/// Returns null or the new alias for the database field name
		/// </summary>
		public string Alias
		{
			get;
			private set;
		}
		
		/// <summary>
		/// Returns the field usage or None
		/// </summary>
		public SqlMapperProperty Property
		{
			get;
			private set;
			
		}
		
		/// <summary>
		/// Constructor to set an alias datebase field name
		/// </summary>
		/// <param name="fieldName">The alias used in result set</param>
		public SqlMapperAttributes(string fieldName)
		{
			if(String.IsNullOrEmpty(fieldName))
				throw new ArgumentNullException("fieldName");
			
			Alias = fieldName;
			Property = SqlMapperProperty.None;
		}
		
		/// <summary>
		/// Constructor to set a property usage flag
		/// </summary>
		/// <param name="property">The property flag</param>
		public SqlMapperAttributes(SqlMapperProperty property)
		{
			Alias = null;
			Property = property;
		}
		
		/// <summary>
		/// Constructor to set an alias datebase field name and a property usage flag
		/// </summary>
		/// <param name="fieldName">The alias used in result set</param>
		/// <param name="property">The property flag</param>
		public SqlMapperAttributes(string fieldName, SqlMapperProperty property)
		{
			if (String.IsNullOrEmpty(fieldName))
				throw new ArgumentNullException("fieldName");
			
			Alias = fieldName;
			Property = property;
		}
	}
	
	/// <summary>
	/// Delegate for <c>SqlObject</c> debugging.
	/// </summary>
	/// <param name="reader"></param>
	public delegate void SqlMapperDebugCallback(DbDataReader reader);
	
	/// <summary>
	/// Provides an attribute for debugging.
	/// </summary>
	[AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
	public class SqlMapperObjectDebug: Attribute
	{
		/// <summary>
		/// Get or set enabled debugging
		/// </summary>
		public bool Enabled
		{
			get;
			private set;
		}
		
		/// <summary>
		/// Get or set the callback function.
		/// </summary>
		public SqlMapperDebugCallback Callback
		{
			get;
			private set;
		}
		
		/// <summary>
		/// Enable debugging of SqlMapper.
		/// The method must: public, static and in an class.
		/// <c>The method has the following signature: 'public static void NAME(DbDataReader reader)'</c>
		/// </summary>
		/// <param name="enable">Enable or disable callback</param>
		/// <param name="type">Typeof class contains the callback</param>
		/// <param name="method">The Name of the callback method</param>
		public SqlMapperObjectDebug(bool enable, Type type, string method)
		{
			Enabled = enable;
			
			try
			{	
				MethodInfo mi = type.GetMethod(method, BindingFlags.Public | BindingFlags.Static | BindingFlags.Instance);
				SqlMapperDebugCallback callback = (SqlMapperDebugCallback)Delegate.CreateDelegate(typeof(SqlMapperDebugCallback), mi);
				
				Callback = callback;
			}
			catch(Exception ex)
			{
				Callback = null;
				throw new SqlDataMapperException("Can't create callback function.", ex);
			}
		}
	}
}
