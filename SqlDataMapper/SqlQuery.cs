using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Collections;
using System.Globalization;

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

		private string Query { get; set; }

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
				return Query;
			}
			private set
			{
				Query = value;
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
		/// Check for empty parameter names and return a list of them.
		/// </summary>
		public IEnumerable<string> GetUnresolvedParameters()
		{
			MatchCollection mc = Regex.Matches(this.QueryString, "@([a-zA-Z0-9_]+)", RegexOptions.Singleline);
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
				var keys = GetUnresolvedParameters();
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
		
		/// <summary>
		/// Convert the source object and try to replace all named parameters.
		/// </summary>
		/// <typeparam name="TSource">The object type</typeparam>
		/// <param name="source">The object</param>
		/// <returns>This instance</returns>
		public SqlQuery SetParameter<TSource>(TSource source) where TSource: class, new()
		{
			if(source == null)
				throw new ArgumentNullException("source");
			
			return SetParameter(SqlObject.GetAsParameter<TSource>(source));
		}
		
		/// <summary>
		/// Convert the parameter object and try to replace all named parameters.
		/// </summary>
		/// <param name="parameters">A SqlParameter object</param>
		/// <returns>This instance</returns>
		public SqlQuery SetParameter(SqlParameter parameters)
		{
			if (parameters == null)
				throw new ArgumentException("parameters");
			
			foreach(DictionaryEntry entry in parameters)
			{
				QueryString = Replace((string)entry.Key, GetValue(entry.Value), true);
			}

			return this;
		}
		
		/// <summary>
		/// Set a single named parameter.
		/// </summary>
		/// <param name="name">The named parameter</param>
		/// <param name="value">The value</param>
		/// <returns>The instance</returns>
		public SqlQuery SetParameter(string name, object value)
		{
			if(String.IsNullOrEmpty(name))
				throw new ArgumentNullException("name");
			
			this.QueryString = Replace(name, GetValue(value));

			return this;
		}

		/// <summary>
		/// Set a single named parameter.
		/// </summary>
		/// <param name="name">The named parameter</param>
		/// <param name="value">The value</param>
		/// <returns>The instance</returns>
		public SqlQuery SetParameter<TSource>(string name, TSource value) // where TSource : IConvertible
		{
			if (String.IsNullOrEmpty(name))
				throw new ArgumentNullException("name");

			if (value as IConvertible == null && value as IEnumerable == null)
				throw new ArgumentException("value is neither a IConvertible or IEnumerable.");

			this.QueryString = Replace(name, GetValue(value));

			return this;
		}

		/// <summary>
		/// Set a single named parameter.
		/// </summary>
		/// <param name="name">The named parameter</param>
		/// <param name="value">The value</param>
		/// <returns>This instance</returns>
		public SqlQuery SetParameter(string name, string value)
		{
			if (String.IsNullOrEmpty(name))
				throw new ArgumentNullException("name");

			this.QueryString = Replace(name, GetValue(value));

			return this;
		}

		/// <summary>
		/// Set a single named parameter.
		/// </summary>
		/// <param name="name">The named parameter</param>
		/// <param name="value">The value</param>
		/// <returns>This instance</returns>
		public SqlQuery SetParameter(string name, int value)
		{
			if (String.IsNullOrEmpty(name))
				throw new ArgumentNullException("name");

			this.QueryString = Replace(name, GetValue(value));

			return this;
		}

		/// <summary>
		/// Set a single named parameter.
		/// </summary>
		/// <param name="name">The named parameter</param>
		/// <param name="value">The value</param>
		/// <returns>This instance</returns>
		public SqlQuery SetParameter(string name, long value)
		{
			if (String.IsNullOrEmpty(name))
				throw new ArgumentNullException("name");

			this.QueryString = Replace(name, GetValue(value));

			return this;
		}

		/// <summary>
		/// Set a single named parameter.
		/// </summary>
		/// <param name="name">The named parameter</param>
		/// <param name="value">The value</param>
		/// <returns>This instance</returns>
		public SqlQuery SetParameter(string name, float value)
		{
			if (String.IsNullOrEmpty(name))
				throw new ArgumentNullException("name");

			this.QueryString = Replace(name, GetValue(value));

			return this;
		}

		/// <summary>
		/// Set a single named parameter.
		/// </summary>
		/// <param name="name">The named parameter</param>
		/// <param name="value">The value</param>
		/// <returns>This instance</returns>
		public SqlQuery SetParameter(string name, double value)
		{
			if (String.IsNullOrEmpty(name))
				throw new ArgumentNullException("name");

			this.QueryString = Replace(name, GetValue(value));

			return this;
		}

		/// <summary>
		/// Set a single named parameter.
		/// </summary>
		/// <param name="name">The named parameter</param>
		/// <param name="value">The value</param>
		/// <returns>This instance</returns>
		public SqlQuery SetParameter(string name, decimal value)
		{
			if (String.IsNullOrEmpty(name))
				throw new ArgumentNullException("name");

			this.QueryString = Replace(name, GetValue(value));

			return this;
		}

		/// <summary>
		/// Set a single named parameter.
		/// </summary>
		/// <param name="name">The named parameter</param>
		/// <param name="value">The value</param>
		/// <returns>This instance</returns>
		public SqlQuery SetParameter(string name, char value)
		{
			if (String.IsNullOrEmpty(name))
				throw new ArgumentNullException("name");

			this.QueryString = Replace(name, GetValue(value));

			return this;
		}
		
		/// <summary>
		/// Set a single named parameter.
		/// </summary>
		/// <param name="name">The named parameter</param>
		/// <param name="value">The value</param>
		/// <returns>This instance</returns>
		public SqlQuery SetParameter(string name, DateTime value)
		{
			if (String.IsNullOrEmpty(name))
				throw new ArgumentNullException("name");

			this.QueryString = Replace(name, GetValue(value));

			return this;
		}

		/// <summary>
		/// Set a single named parameter.
		/// </summary>
		/// <param name="name">The named parameter</param>
		/// <param name="value">The value</param>
		/// <returns>The instance</returns>
		public SqlQuery SetParameter(string name, Guid value)
		{
			if (String.IsNullOrEmpty(name))
				throw new ArgumentNullException("name");

			this.QueryString = Replace(name, GetValue(value));

			return this;
		}

		
		/// <summary>
		/// Set a single named parameter.
		/// </summary>
		/// <param name="name">The named parameter</param>
		/// <param name="value">The value</param>
		/// <returns>This instance</returns>
		public SqlQuery SetParameter(string name, byte[] value)
		{
			if (String.IsNullOrEmpty(name))
				throw new ArgumentNullException("name");

			if (value == null)
				throw new ArgumentNullException("value");

			this.QueryString = Replace(name, GetValue(value));

			return this;
		}
		
		/// <summary>
		/// Add sql query object and return a new instance.
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
		/// Add sql query string and return a new instance.
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
		/// Appends sql query object to this instance.
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
		/// Appends sql query string to this instance.
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
		/// <param name="suppressException">Suppress a exception if a paramater not found.</param>
		/// <returns>The replaced sql query</returns>
		private string Replace(string name, object value, bool suppressException)
		{
			if (String.IsNullOrEmpty(name))
				throw new ArgumentNullException("name");

			int matchCount = 0;
			string newText = Regex.Replace(this.QueryString, String.Format("@{0}", name.Trim()), match =>
			{
				matchCount++;
				return match.Result(String.Format(CultureInfo.InvariantCulture.NumberFormat, "{0}", value));
			}, RegexOptions.IgnoreCase);
			
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

			//remove comments
			//tmp = Regex.Replace(tmp, @"(--.*)$", " ", RegexOptions.Multiline);
			//tmp = Regex.Replace(tmp, @"(/\*.*?\*/)", " ", RegexOptions.Singleline);

			return tmp;
		}

		/// <summary>
		/// Convert the the value to a sql type. Enumerables, <remarks>excluding strings and byte-arrays</remarks>, will transformed to comma separated line.
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
		/// <param name="value">The value.</param>
		/// <returns>A sql compatible string.</returns>
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
				return String.Format("0x{0}", BitConverter.ToString(value as byte[]).Replace("-", String.Empty));
			}

			if (value.GetType() == typeof(DateTime))
			{
				return String.Format("'{0:yyyy-MM-dd HH:mm:ss}'", value);
			}

			if (value.GetType() == typeof(Guid))
			{
				return String.Format("'{0}'", value);
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
