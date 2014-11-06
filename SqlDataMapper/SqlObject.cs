using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;
using System.Data.Common;
using System.Collections.Specialized;
using System.Diagnostics;

namespace SqlDataMapper
{
	/// <summary>
	/// Convert a data reader to class and class to SqlParameter class.
	/// </summary>
	public class SqlObject
	{
		/// <summary>
		/// Map DBDataReader to class.
		/// </summary>
		/// <typeparam name="TDestination">The target class.</typeparam>
		/// <param name="dataReader">The data reader object.</param>
		/// <returns>New instance of TDestination.</returns>
		static public TDestination GetAs<TDestination>(DbDataReader dataReader) where TDestination: class, new()
		{
			#region Object debug
			SqlMapperDebugAttribute debugger = GetObjectAttribute<TDestination>();
			if (debugger.Enabled)
			{
				if (debugger.Callback != null)
				{
					debugger.Callback(dataReader);
				}
			}
			#endregion
			
			TDestination newObject = Activator.CreateInstance<TDestination>();
			PropertyInfo[] properties = typeof(TDestination).GetProperties();
			var columns = ColumnNames(dataReader);
			
			foreach(var property in properties)
			{
				SqlMapperInAttribute attr = GetPropertyAttribute<SqlMapperInAttribute>(property);

				if (attr.Flag == SqlMapperFlags.Ignore)
				{
					continue;
				}

				string columnName = property.Name;
				if (!String.IsNullOrEmpty(attr.Alias))
				{
					columnName = attr.Alias;
				}

				if(columns.Contains(columnName.ToLower()))
				{
					object value = GetValue(dataReader[columnName]);

					if (attr.Flag == SqlMapperFlags.NotNull && value == null)
					{
						throw new SqlDataMapperException(String.Format("The property {0}.{1} must not be null.", newObject.GetType().FullName, columnName));
					}

					try
					{
						typeof(TDestination).InvokeMember(property.Name, BindingFlags.SetProperty, null, newObject, new object[] { value });
					}
					catch (Exception ex)
					{
						if (ex is MissingMethodException)
							throw new SqlDataMapperException(String.Format("The property {0}.{1} has the wrong type. Type '{2}' required", newObject.GetType().FullName, columnName, dataReader[columnName].GetType()), ex);
						else
							throw;
					}
				}
				else if (attr.Flag == SqlMapperFlags.Required || attr.Flag == SqlMapperFlags.NotNull)
				{
					throw new SqlDataMapperException(String.Format("The property {0}.{1} is required and must not be null.", newObject.GetType().FullName, columnName));
				}
			}

			return newObject;
		}

		/// <summary>
		/// Convert a class to a SqlParameter class.
		/// </summary>
		/// <typeparam name="TSource">The source class.</typeparam>
		/// <param name="source">The source.</param>
		/// <returns>New instance of SqlParameter</returns>
		static public SqlParameter GetAsParameter<TSource>(TSource source) where TSource: class, new()
		{
			SqlParameter param = new SqlParameter();

			PropertyInfo[] properties = typeof(TSource).GetProperties();
			foreach (var property in properties)
			{
				SqlMapperOutAttribute attr = GetPropertyAttribute<SqlMapperOutAttribute>(property);

				if (attr.Flag == SqlMapperFlags.Ignore)
				{
					continue;
				}

				string columnName = property.Name;
				if (!String.IsNullOrEmpty(attr.Alias))
				{
					columnName = attr.Alias;
				}

				object value = typeof(TSource).InvokeMember(property.Name, BindingFlags.GetProperty, null, source, null);

				if(value == null && (attr.Flag == SqlMapperFlags.NotNull || attr.Flag == SqlMapperFlags.Required))
				{
					throw new SqlDataMapperException(String.Format("The property {0}.{1} must not be null.", source.GetType().FullName, columnName));
				}

				param.Add(columnName, value);
			}
			
			return param;
		}

		/// <summary>
		/// Convert DBNull to null.
		/// </summary>
		/// <param name="val">A value</param>
		/// <returns>A converted value.</returns>
		private static object GetValue(object val)
		{
			if (val == DBNull.Value)
			{
				return null;
			}
			return val;
		}

		/// <summary>
		/// Get property of a member.
		/// </summary>
		/// <typeparam name="TProperty">The target property.</typeparam>
		/// <param name="propertyInfo">The property info.</param>
		/// <returns>The property.</returns>
		static private TProperty GetPropertyAttribute<TProperty>(PropertyInfo propertyInfo) where TProperty: ISqlMapperAttribute, new()
		{
			object[] attr = propertyInfo.GetCustomAttributes(typeof(TProperty), true);
			if (attr != null && attr.Length > 0)
			{
				return (TProperty)attr[0];
			}
			return new TProperty();
		}

		/// <summary>
		/// Get property of a class.
		/// </summary>
		/// <typeparam name="TSource">The source class.</typeparam>
		/// <returns>The attribute.</returns>
		static private SqlMapperDebugAttribute GetObjectAttribute<TSource>() where TSource : class, new()
		{
			object[] attr = typeof(TSource).GetCustomAttributes(typeof(SqlMapperDebugAttribute), true);
			if (attr != null && attr.Length > 0)
			{
				return attr[0] as SqlMapperDebugAttribute;
			}
			return new SqlMapperDebugAttribute();
		}

		/// <summary>
		/// Get the column names of DbDataReader object.
		/// </summary>
		/// <param name="dataReader"></param>
		/// <returns></returns>
		static private IEnumerable<string> ColumnNames(DbDataReader dataReader)
		{
			for (int i = 0; i < dataReader.FieldCount; ++i)
			{
				yield return dataReader.GetName(i).ToLower();
			}
		}
	}
}
