using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using System.Reflection;
using System.Text;

namespace SqlDataMapper
{
	/// <summary>
	/// Class to map DbDataReader row to an object.
	/// </summary>
	/// <typeparam name="T">The destination type.</typeparam>
	internal sealed class SqlMapper<T> where T : class, new()
	{
		/// <summary>
		/// A list of all columns.
		/// </summary>
		private List<Column> Columns = new List<Column>();

		/// <summary>
		/// Result columns.
		/// </summary>
		private class TableColumn
		{
			public string Name { get; set; }
			public int Index { get; set; }
		}

		/// <summary>
		/// Initialize sql mapper.
		/// </summary>
		public SqlMapper()
		{
			var properties = typeof(T).GetProperties(BindingFlags.SetProperty | BindingFlags.GetProperty | BindingFlags.Public | BindingFlags.Instance);

			foreach (var property in properties)
			{
				var ignore = property.GetCustomAttributes(typeof(IgnoreAttribute), true).Length > 0;
				if (!ignore && property.CanWrite)
				{
					Columns.Add(new Column(property));
				}
			}
		}

		/// <summary>
		/// Map DbDataReader to an object.
		/// </summary>
		/// <param name="dataReader"></param>
		/// <returns></returns>
		public T MapFrom(IDataReader dataReader)
		{
			T newObject = Activator.CreateInstance<T>();
			var tableColumns = TableColumns(dataReader);

			foreach(var column in Columns)
			{
				var tableColumn = tableColumns.FirstOrDefault(f => f.Name.Equals(column.Name, StringComparison.OrdinalIgnoreCase));
				if(tableColumn != null)
				{
					var value = GetValue(dataReader.GetValue(tableColumn.Index));

					if (column.NotNull && value == null)
					{
						throw new SqlDataMapperException(String.Format("The property {0}.{1} must not be null.", newObject.GetType().FullName, column.PropertyName));
					}

					try
					{
						column.SetValue(newObject, value);
					}
					catch(Exception ex)
					{
						throw new SqlDataMapperException(String.Format("The property {0}.{1} has the wrong type. Type '{2}' required", newObject.GetType().FullName, column.PropertyName, dataReader.GetValue(tableColumn.Index).GetType()), ex);
					}
				}
				else if (column.IsRequired || column.NotNull)
				{
					throw new SqlDataMapperException(String.Format("The property {0}.{1} is required and must not be null.", newObject.GetType().FullName, column.PropertyName));
				}
			}

			return newObject;
		}

		/// <summary>
		/// Check if value is DBNull and return null instead.
		/// </summary>
		/// <param name="value"></param>
		/// <returns></returns>
		private object GetValue(object value)
		{
			if (value == DBNull.Value)
			{
				return null;
			}
			return value;
		}

		/// <summary>
		/// Get a list of all result columns.
		/// </summary>
		/// <param name="dataReader"></param>
		/// <returns></returns>
		private IEnumerable<TableColumn> TableColumns(IDataReader dataReader)
		{
			for (int i = 0; i < dataReader.FieldCount; ++i)
			{
				yield return new TableColumn { Name = dataReader.GetName(i), Index = i };
			}
		}
	}

	/// <summary>
	/// Class for object columns.
	/// </summary>
	internal sealed class Column
	{
		/// <summary>
		/// The property of the column.
		/// </summary>
		private PropertyInfo Property { get; set; }
		
		/// <summary>
		/// The name of the property.
		/// </summary>
		public string PropertyName { get; private set; }

		/// <summary>
		/// The column name.
		/// </summary>
		public string Name { get; private set; }

		/// <summary>
		/// The value is required in result.
		/// </summary>
		public bool IsRequired { get; private set; }

		/// <summary>
		/// The value must not null in result.
		/// </summary>
		public bool NotNull { get; private set; }

		/// <summary>
		/// Initialize a new column definition.
		/// </summary>
		/// <param name="property"></param>
		public Column(PropertyInfo property)
		{
			PropertyName = property.Name;
			Name = property.Name;
			Property = property;

			IsRequired = property.GetCustomAttributes(typeof(RequiredAttribute), true).Length > 0;
			NotNull = property.GetCustomAttributes(typeof(NotNullAttribute), true).Length > 0;
			
			var alias = property.GetCustomAttributes(typeof(AliasAttribute), true).Cast<AliasAttribute>().FirstOrDefault();
			if (alias != null && !String.IsNullOrEmpty(alias.Name))
			{
				Name = alias.Name;
			}
		}

		/// <summary>
		/// Set the property value.
		/// </summary>
		/// <param name="target"></param>
		/// <param name="value"></param>
		public void SetValue(object target, object value)
		{
			Property.SetValue(target, value, null);
		}

		/// <summary>
		/// Get the property value.
		/// </summary>
		/// <param name="source"></param>
		/// <returns></returns>
		public object GetValue(object source)
		{
			return Property.GetValue(source, null);
		}
	}
}
