using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Collections;

namespace SqlDataMapper
{
	/// <summary>
	/// The implementaton of the query object.
	/// </summary>
	public class SqlQuery: ISqlQuery
	{
		/// <summary>
		/// The format handler type to override the internal handler.
		/// </summary>
		/// <param name="value"></param>
		/// <returns></returns>
		public delegate object FormatHandler(object value);

		private string _query;

		/// <summary>
		/// Override the internal format handler.
		/// </summary>
		public FormatHandler Handler { get; set; }
		
		/// <summary>
		/// Get the query string
		/// </summary>
		public string QueryString
		{
			get
			{
				return _query;
			}
			private set
			{
				_query = value;
			}
		}
		
		/// <summary>
		/// Create an new sql query object with an empty query
		/// </summary>
		public SqlQuery()
			: this("")
		{ }

		/// <summary>
		/// Create a new sql query from an other instance
		/// </summary>
		/// <param name="query">A ISqlQuery instance</param>
		public SqlQuery(ISqlQuery query)
			: this(query.QueryString)
		{ }
		
		/// <summary>
		/// Create a new sql query.
		/// </summary>
		/// <param name="query">A sql query</param>
		public SqlQuery(string query)
		{
			if(query == null)
				throw new ArgumentNullException("query");
				
			this.QueryString = Format(query);
		}
		
		/// <summary>
		/// Appends a sql query string
		/// </summary>
		static public SqlQuery operator +(SqlQuery query, string queryToAdd)
		{
			return query.Add(queryToAdd);
		}
		
		/// <summary>
		/// Appends a sql query object
		/// </summary>
		static public SqlQuery operator +(SqlQuery query, ISqlQuery queryToAdd)
		{
			return query.Add(queryToAdd);
		}

		/// <summary>
		/// Check for empty parameter names an return a list of them.
		/// </summary>
		private IEnumerable<string> CheckForUnresolvedParameters()
		{
			MatchCollection mc = Regex.Matches(this.QueryString, "#([a-zA-Z0-9_]+)?#", RegexOptions.Singleline);
			if (mc.Count > 0)
			{
				foreach (Match m in mc)
				{
					yield return m.Groups[1].Value;
				}
			}
		}

		/// <summary>
		/// Check for empty parameter names an throws an exception if found any
		/// </summary>
		public ISqlQuery Check()
		{
			return Check(true);
		}

		/// <summary>
		/// Check for empty parameter names an throws an exception if found any.
		/// </summary>
		/// <param name="check">Perform the check if set to true.</param>
		public ISqlQuery Check(bool check)
		{
			if(check)
			{
				string[] keys = CheckForUnresolvedParameters().ToArray();
				if (keys.Count() > 0)
				{
					StringBuilder sb = new StringBuilder();
					foreach (string key in keys)
					{
						if (sb.Length > 0)
							sb.Append(", ");
						sb.Append(key);
					}
					throw new SqlDataMapperException(String.Format("The sql statement has unresolved parameters. -> {0}", sb.ToString()));
				}
			}
			return this;
		}

		/// <summary>
		/// Get a SqlParameter object for unresolved parameters.
		/// </summary>
		public SqlParameter GetUnresolvedParameters()
		{
			return this.GetUnresolvedParameters("");
		}

		/// <summary>
		/// Get a SqlParameter object for unresolved parameters.
		/// </summary>
		/// <param name="value">A custom value for each parameter</param>
		public SqlParameter GetUnresolvedParameters(object value)
		{
			SqlParameter param = new SqlParameter();

			foreach (string key in CheckForUnresolvedParameters())
			{
				param.Add(key, value);
			}

			return param;
		}
		
		/// <summary>
		/// Replaces the whole existing query with the new query
		/// </summary>
		/// <param name="query">The new query string</param>
		/// <returns>This instance</returns>
		public ISqlQuery Set(string query)
		{
			this.QueryString = Format(query);
			return this;
		}
		
		/// <summary>
		/// Create a new dynamic sql query
		/// </summary>
		/// <param name="query">A query string</param>
		/// <returns>A new instance</returns>
		static public SqlQuery CreateQuery(string query)
		{
			return new SqlQuery(query);
		}
		
		/// <summary>
		/// Create a new dynamic sql query out of an other query
		/// </summary>
		/// <param name="query">A query object</param>
		/// <returns>A new instance</returns>
		static public SqlQuery CreateQuery(ISqlQuery query)
		{
			return CreateQuery(query.QueryString);
		}
		
		#region Public methods
		
		/// <summary>
		/// Convert the object to a parameter object and try to replace all named parameters
		/// </summary>
		/// <typeparam name="T">The object type</typeparam>
		/// <param name="obj">The object</param>
		/// <returns>This instance</returns>
		public SqlQuery SetEntities<T>(T obj)
		{
			if(obj == null)
				throw new ArgumentNullException("obj");
			
			return SetEntities(SqlObject.GetParameters<T>(obj));
		}
		
		/// <summary>
		/// Convert the parameter object and try to replace all named parameters
		/// </summary>
		/// <param name="parameters">A SqlParameter object</param>
		/// <returns>This instance</returns>
		public SqlQuery SetEntities(SqlParameter parameters)
		{
			if(parameters != null)
			{
				foreach(DictionaryEntry entry in parameters)
				{
					QueryString = Replace((string)entry.Key, GetValue(entry.Value), true);
				}
			}
			return this;
		}
		
		/// <summary>
		/// Set a single named parameter. The value can be every type.
		/// </summary>
		/// <param name="name"></param>
		/// <param name="value"></param>
		/// <returns></returns>
		public SqlQuery SetEntity(string name, object value)
		{
			if(String.IsNullOrEmpty(name))
				throw new ArgumentNullException("name");
			
			this.QueryString = Replace(name, GetValue(value));
			return this;
		}

		/// <summary>
		/// Set a single named parameter. The value must a string
		/// </summary>
		/// <param name="name">The named parameter</param>
		/// <param name="value">The value</param>
		/// <returns>This instance</returns>
		public SqlQuery SetString(string name, string value)
		{
			if (String.IsNullOrEmpty(name))
				throw new ArgumentNullException("name");

			this.QueryString = Replace(name, GetValue(value));
			return this;
		}

		/// <summary>
		/// Set a single named parameter. The value must a integer
		/// </summary>
		/// <param name="name">The named parameter</param>
		/// <param name="value">The value</param>
		/// <returns>This instance</returns>
		public SqlQuery SetInt(string name, int value)
		{
			if (String.IsNullOrEmpty(name))
				throw new ArgumentNullException("name");

			this.QueryString = Replace(name, GetValue(value));
			return this;
		}

		/// <summary>
		/// Set a single named parameter. The value must a long
		/// </summary>
		/// <param name="name">The named parameter</param>
		/// <param name="value">The value</param>
		/// <returns>This instance</returns>
		public SqlQuery SetLong(string name, long value)
		{
			if (String.IsNullOrEmpty(name))
				throw new ArgumentNullException("name");

			this.QueryString = Replace(name, GetValue(value));
			return this;
		}

		/// <summary>
		/// Set a single named parameter. The value must a single
		/// </summary>
		/// <param name="name">The named parameter</param>
		/// <param name="value">The value</param>
		/// <returns>This instance</returns>
		public SqlQuery SetSingle(string name, float value)
		{
			if (String.IsNullOrEmpty(name))
				throw new ArgumentNullException("name");

			this.QueryString = Replace(name, GetValue(value));
			return this;
		}

		/// <summary>
		/// Set a single named parameter. The value must a double
		/// </summary>
		/// <param name="name">The named parameter</param>
		/// <param name="value">The value</param>
		/// <returns>This instance</returns>
		public SqlQuery SetDouble(string name, double value)
		{
			if (String.IsNullOrEmpty(name))
				throw new ArgumentNullException("name");

			this.QueryString = Replace(name, GetValue(value));
			return this;
		}

        /// <summary>
        /// Set a single named parameter. The value must a decimal
        /// </summary>
        /// <param name="name">The named parameter</param>
        /// <param name="value">The value</param>
        /// <returns>This instance</returns>
        public SqlQuery SetDecimal(string name, decimal value)
        {
            if (String.IsNullOrEmpty(name))
				throw new ArgumentNullException("name");

			this.QueryString = Replace(name, GetValue(value));
			return this;
        }

		/// <summary>
		/// Set a single named parameter. The value must a char
		/// </summary>
		/// <param name="name">The named parameter</param>
		/// <param name="value">The value</param>
		/// <returns>This instance</returns>
		public SqlQuery SetChar(string name, char value)
		{
			if (String.IsNullOrEmpty(name))
				throw new ArgumentNullException("name");

			this.QueryString = Replace(name, GetValue(value));
			return this;
		}
		
		/// <summary>
		/// Set a single named parameter. The value must a DateTime
		/// </summary>
		/// <param name="name">The named parameter</param>
		/// <param name="value">The value</param>
		/// <returns>This instance</returns>
		public SqlQuery SetDateTime(string name, DateTime value)
		{
			if (String.IsNullOrEmpty(name))
				throw new ArgumentNullException("name");

			this.QueryString = Replace(name, GetValue(value));
			return this;
		}
		
		/// <summary>
		/// Set a single named parameter. The value must a binary array.
		/// </summary>
		/// <param name="name">The named parameter</param>
		/// <param name="value">The value</param>
		/// <returns>This instance</returns>
		public SqlQuery SetBinary(string name, byte[] value)
		{
			if (String.IsNullOrEmpty(name))
				throw new ArgumentNullException("name");

			if (value == null)
				throw new ArgumentNullException("value");

			this.QueryString = Replace(name, GetValue(value));
			return this;
		}
		
		/// <summary>
		/// Add a sql query object
		/// </summary>
		/// <param name="query">A sql object contains the query</param>
		/// <returns>A new instance</returns>
		public SqlQuery Add(ISqlQuery query)
		{
			if (query == null)
				throw new ArgumentNullException("query");
			
			return Add(query.QueryString);
		}
		
		/// <summary>
		/// Add a sql query string
		/// </summary>
		/// <param name="query">A query or a fragment of a query</param>
		/// <returns>A new instance</returns>
		public SqlQuery Add(string query)
		{
			if (String.IsNullOrEmpty(query))
				throw new ArgumentNullException("query");
			
			return new SqlQuery(String.Format("{0} {1}", this.QueryString, query));
		}

		/// <summary>
		/// Appends a sql query object
		/// </summary>
		/// <param name="query">A sql object contains the query</param>
		/// <returns>This instance</returns>
		public SqlQuery Append(ISqlQuery query)
		{
			if (query == null)
				throw new ArgumentNullException("query");

			return Append(query.QueryString);
		}

		/// <summary>
		/// Appends a sql query string
		/// </summary>
		/// <param name="query">A query or a fragment of a query</param>
		/// <returns>This instance</returns>
		public SqlQuery Append(string query)
		{
			if (String.IsNullOrEmpty(query))
				throw new ArgumentNullException("query");

			this.QueryString = Format(String.Format("{0} {1}", this.QueryString, query));
			return this;
		}
		#endregion
		
		/// <summary>
		/// Get the sql query
		/// </summary>
		public override string ToString()
		{
			return this.QueryString;
		}
		
		
		#region Private methods

		/// <summary>
		/// Replace the parameters with the value.
		/// </summary>
		/// <param name="name">The parameter name.</param>
		/// <param name="value">The value. Pass it before through GetValue()</param>
		/// <param name="suppressException"></param>
		/// <returns>The replaced sql query</returns>
		private string Replace(string name, object value, bool suppressException)
		{
			if (String.IsNullOrEmpty(name))
				throw new ArgumentNullException("name");

			int matchCount = 0;
			string newText = Regex.Replace(this.QueryString, String.Format("#{0}#", name), match =>
			{
				matchCount++;
				return match.Result(String.Format("{0}", value));
			}, RegexOptions.None);
			
			if(!suppressException)
			{
				if (matchCount == 0)
				{
					throw new SqlDataMapperException(String.Format("No named parameter found for name: '{0}'.", name));
				}
			}

			return newText;
		}
		
		/// <summary>
		/// Replace the parameters with the value.
		/// </summary>
		/// <param name="name">The parameter name.</param>
		/// <param name="value">The value. Pass it before through GetValue()</param>
		/// <returns>The replaced sql query</returns>
		private string Replace(string name, object value)
		{
			return Replace(name, value, false);
		}

		/// <summary>
		/// Remove comments (line and block)
		/// </summary>
		/// <param name="query">A sql string</param>
		/// <returns>A formatted sql string</returns>
		private string Format(string query)
		{
            string tmp = query;

            //remove coments
            tmp = Regex.Replace(tmp, @"(--.*)$", " ", RegexOptions.Multiline);
            tmp = Regex.Replace(tmp, @"(/\*.*?\*/)", " ", RegexOptions.Singleline);

            return tmp;
		}

		/// <summary>
		/// Convert the the value to a sql type. Enumerables, exclude strings and byte-arrays, will transformed to comma separated line.
		/// </summary>
		/// <param name="value">The value</param>
		/// <returns>A sql compatible string</returns>
		private object GetValue(object value)
		{
			IEnumerable enumerable = value as IEnumerable;
			if (enumerable != null && value.GetType() != typeof(string) && value.GetType() != typeof(byte[]))
			{
				StringBuilder sb = new StringBuilder();
				foreach (var element in enumerable)
				{
					if (sb.Length > 0)
						sb.Append(", ");
					sb.Append(GetPrimitive(element));
				}

				return sb.ToString();
			}

			return GetPrimitive(value);
		}

		/// <summary>
		/// Format the value to compatible sql string.
		/// </summary>
		/// <param name="value">The value</param>
		/// <returns>A sql compatible string</returns>
		private object GetPrimitive(object value)
		{
			if (this.Handler != null)
				return this.Handler(value);
			
			if (value == null)
			{
				return String.Format("null");
			}

			if (value.GetType() == typeof(bool))
			{
				return (bool)value ? 1 : 0;
			}

			if (value.GetType() == typeof(byte[]))
			{
				return ("0x" + BitConverter.ToString(value as byte[]).Replace("-", String.Empty));
			}

			if (value.GetType() == typeof(DateTime))
			{
				return String.Format("'{0:yyyy-MM-dd HH:mm:ss}'", value);
			}

			if (value.GetType() == typeof(string))
			{
				return String.Format("'{0}'", value);
			}

			if (value.GetType() == typeof(char))
			{
				return String.Format("'{0}'", value);
			}

			return value;
		}
		#endregion
	}
}
