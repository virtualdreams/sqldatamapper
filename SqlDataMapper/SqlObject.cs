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
	/// The core class for sql-to-class mapping and class-to-<c>SqlParameter</c> mapping.
	/// </summary>
	public class SqlObject
	{
		#region Obsolete
		//[Obsolete("This function get removed in the near future.", true)]
		//public static object GetAs(DbDataReader reader, Type objectToReturn)
		//{
		//    object newObject = Activator.CreateInstance(objectToReturn);
		//    PropertyInfo[] props = objectToReturn.GetProperties();
		//    for(int i = 0; i < props.Length; ++i)
		//    {
		//        if(ColumnExists(reader, props[i].Name) /*&& reader[props[i].Name] != DBNull.Value*/)
		//        {
		//            //Console.WriteLine(reader[props[i].Name].GetType());
		//            objectToReturn.InvokeMember(props[i].Name, BindingFlags.SetProperty, null, newObject, new object[] { GetDbValue(reader[props[i].Name]) });
		//        }
		//    }
		//    return newObject;
		//}
		#endregion

		/// <summary>
		/// Map a data reader object to a class
		/// </summary>
		public static T GetAs<T>(DbDataReader reader)
		{
			#region Debugcode
			SqlMapperObjectDebug od = GetObjectAttribute<T>();
			if(od != null && od.Enabled)
			{
				if(od.Callback != null)
				{
					od.Callback(reader);
				}
			}
			#endregion
			
			T newObject = Activator.CreateInstance<T>();
			PropertyInfo[] props = typeof(T).GetProperties();
			
			for(int i = 0; i < props.Length; ++i)
			{
				SqlMapperAttributes attr = GetPropertyAttribute(props[i]);
				
				//if the property ignored, then jump over
				if(attr != null && SqlMapperProperty.Ignored == (attr.Property & SqlMapperProperty.Ignored))
				{
					continue;
				}
				
				//if an alternate fieldname available, then use the alternate
				string colName = props[i].Name;
				if(attr != null && !String.IsNullOrEmpty(attr.FieldName))
					colName = attr.FieldName;
				
				if (ColumnExists(reader, colName))
				{
					object obj = GetDbValue(reader[colName]);
					
					//if sql field is null and attribute must not null, then throw an exception
					if (attr != null && SqlMapperProperty.NotNull == (attr.Property & SqlMapperProperty.NotNull) && obj == null)
					{
						throw new SqlDataMapperException(String.Format("The property {0}.{1} can't set to a null value.", newObject.GetType().FullName, colName));
					}
					
					//try to set the value on the property
					try
					{		
						typeof(T).InvokeMember(props[i].Name, BindingFlags.SetProperty, null, newObject, new object[] { obj });
					}
					catch (Exception ex)
					{
						if(ex is MissingMethodException)
							throw new SqlDataMapperException(String.Format("Property {0}.{1} has the wrong type. Type '{2}' required", newObject.GetType().FullName, colName, reader[colName].GetType()), ex); 
						else
							throw;
					}
				}
				else if (attr != null && (SqlMapperProperty.Required == (attr.Property & SqlMapperProperty.Required) || SqlMapperProperty.NotNull == (attr.Property & SqlMapperProperty.NotNull )))
				{
					throw new SqlDataMapperException(String.Format("The property {0}.{1} is required and must exists in result set.", newObject.GetType().FullName, colName));
				}
			}
			return newObject;
		}
		
		/// <summary>
		/// Map an object to <c>SqlParameter</c> class
		/// </summary>
		public static SqlParameter GetParameters<T>(T obj)
		{
			SqlParameter param = new SqlParameter();

			PropertyInfo[] props = typeof(T).GetProperties();
			for(int i = 0; i < props.Length; ++i)
			{
				SqlMapperAttributes attr = GetPropertyAttribute(props[i]);
				if (attr != null && SqlMapperProperty.Ignored == (attr.Property & SqlMapperProperty.Ignored))
				{
					continue;
				}

				//if an alternate fieldname available, then use the alternate
				string colName = props[i].Name;
				if (attr != null && !String.IsNullOrEmpty(attr.FieldName))
					colName = attr.FieldName;
				
				param.Add(colName, typeof(T).InvokeMember(props[i].Name, BindingFlags.GetProperty, null, obj, null));
			}

			return param;
		}

		/// <summary>
		/// Translate DbNull to null
		/// </summary>
		private static object GetDbValue(object val)
		{
			if (val == DBNull.Value)
			{
				return null;
			}
			return val;
		}
		
		/// <summary>
		/// Check if column exists in database result set
		/// </summary>
		private static bool ColumnExists(DbDataReader reader, string columnName)
		{
			for(int i = 0; i < reader.FieldCount; ++i)
			{
				if(columnName.ToLower().Equals(reader.GetName(i).ToLower()))
				{
					return true;
				}
			}
			return false;
		}

		/// <summary>
		/// Get the custom attribute for the property
		/// </summary>
		/// <returns>Returns the attribute or null</returns>
		private static SqlMapperAttributes GetPropertyAttribute(PropertyInfo pInfo)
		{
			object[] attr = pInfo.GetCustomAttributes(typeof(SqlMapperAttributes), true);
			if (attr != null && attr.Length > 0)
			{
				return attr[0] as SqlMapperAttributes;
			}
			return null;
		}
		
		/// <summary>
		/// Get the custom attribute for the class
		/// </summary>
		/// <typeparam name="T">The class object</typeparam>
		/// <returns>Returns the attribute or null</returns>
		private static SqlMapperObjectDebug GetObjectAttribute<T>()
		{
			object[] attr = typeof(T).GetCustomAttributes(typeof(SqlMapperObjectDebug), true);
			if (attr != null && attr.Length > 0)
			{
				return attr[0] as SqlMapperObjectDebug;
			}
			return null;
		}
	}
}
