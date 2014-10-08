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
	public class SqlObject
	{
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

				object value = GetValue(dataReader[columnName]);

				if (attr.Flag == SqlMapperFlags.NotNull && value == null)
				{
					throw new SqlDataMapperException(String.Format("The property {0}.{1} can't set to a null value.", newObject.GetType().FullName, columnName));
				}

				if(columns.Contains(columnName.ToLower()))
				{
					try
					{
						typeof(TDestination).InvokeMember(property.Name, BindingFlags.SetProperty, null, newObject, new object[] { value });
					}
					catch (Exception ex)
					{
						if (ex is MissingMethodException)
							throw new SqlDataMapperException(String.Format("Property {0}.{1} has the wrong type. Type '{2}' required", newObject.GetType().FullName, columnName, dataReader[columnName].GetType()), ex);
						else
							throw;
					}
				}
				else if (attr.Flag == SqlMapperFlags.Required || attr.Flag == SqlMapperFlags.NotNull)
				{
					throw new SqlDataMapperException(String.Format("The property {0}.{1} is required and must exists in result set.", newObject.GetType().FullName, columnName));
				}
			}

			return newObject;
		}

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
					throw new SqlDataMapperException(String.Format("The property {0}.{1} is required and must set.", source.GetType().FullName, columnName));
				}

				param.Add(columnName, value);
			}
			
			return param;
		}

		private static object GetValue(object val)
		{
			if (val == DBNull.Value)
			{
				return null;
			}
			return val;
		}

		static private TProperty GetPropertyAttribute<TProperty>(PropertyInfo propertyInfo) where TProperty: ISqlMapperAttribute
		{
			object[] attr = propertyInfo.GetCustomAttributes(typeof(TProperty), true);
			if (attr != null && attr.Length > 0)
			{
				return (TProperty)attr[0];
			}
			return default(TProperty);
		}

		static private SqlMapperDebugAttribute GetObjectAttribute<TSource>() where TSource : class, new()
		{
			object[] attr = typeof(TSource).GetCustomAttributes(typeof(SqlMapperDebugAttribute), true);
			if (attr != null && attr.Length > 0)
			{
				return attr[0] as SqlMapperDebugAttribute;
			}
			return default(SqlMapperDebugAttribute);
		}

		static private IEnumerable<string> ColumnNames(DbDataReader dataReader)
		{
			for (int i = 0; i < dataReader.FieldCount; ++i)
			{
				yield return dataReader.GetName(i).ToLower();
			}
		}
	}
}
