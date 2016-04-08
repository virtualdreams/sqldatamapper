using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Globalization;

namespace SqlDataMapper
{
	/// <summary>
	/// Class for a sql query.
	/// </summary>
	public class SqlQuery: ISqlQuery
	{
		/// <summary>
		/// Curly bracket parameters.
		/// </summary>
		static private Regex RegexParameter = new Regex(@"\{([a-zA-Z0-9_][a-zA-Z0-9_]+)\}", RegexOptions.Compiled);

		/// <summary>
		/// Regular expression for parameter name.
		/// </summary>
		static private Regex RegexName = new Regex("[a-zA-Z0-9_][a-zA-Z0-9_]+", RegexOptions.Compiled);

		/// <summary>
		/// The query string.
		/// </summary>
		private string Query { get; set; }

		/// <summary>
		/// Default value formatter.
		/// </summary>
		static private ISqlFormatter Formatter = new SqlFormatter();

		/// <summary>
		/// Get the query string
		/// </summary>
		public string QueryString
		{
			get
			{
				return Query.ToString();
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
		/// <param name="query">A sql query.</param>
		public SqlQuery(string query)
		{
			if(query == null)
				throw new ArgumentNullException("query");
				
			this.QueryString = query;
		}
		
		/// <summary>
		/// Appends a sql query string
		/// </summary>
		static public SqlQuery operator +(SqlQuery query, string queryToAdd)
		{
			return query.Add(queryToAdd);
		}
		
		/// <summary>
		/// Appends a sql query object.
		/// </summary>
		static public SqlQuery operator +(SqlQuery query, ISqlQuery queryToAdd)
		{
			return query.Add(queryToAdd);
		}

		/// <summary>
		/// Get all parameters.
		/// </summary>
		public IEnumerable<string> GetParameters()
		{
			foreach (Match match in RegexParameter.Matches(this.QueryString))
			{
				yield return match.Groups[1].Value;
			}
		}

		/// <summary>
		/// Check for empty parameter names an throws an exception if found any.
		/// </summary>
		public SqlQuery Check()
		{
			return Check(true);
		}

		/// <summary>
		/// Check for empty parameter names an throws an exception if found any.
		/// </summary>
		/// <param name="check">Perform the check if set to true.</param>
		public SqlQuery Check(bool check)
		{
			if(check)
			{
				var keys = GetParameters();
				if (keys.Count() > 0)
				{
					StringBuilder sb = new StringBuilder();
					foreach (var key in keys)
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
		/// Replaces the whole existing query string with the new query.
		/// </summary>
		/// <param name="query">The new query string.</param>
		/// <returns>This instance.</returns>
		public ISqlQuery Set(string query)
		{
			if (String.IsNullOrEmpty(query))
				throw new ArgumentNullException("query");

			this.QueryString = query;
			return this;
		}
		
		/// <summary>
		/// Create a new SqlQuery object.
		/// </summary>
		/// <param name="query">A query string.</param>
		/// <returns>A new instance.</returns>
		static public SqlQuery CreateQuery(string query)
		{
			return new SqlQuery(query);
		}
		
		/// <summary>
		/// Create a new SqlQuery object.
		/// </summary>
		/// <param name="query">A query object.</param>
		/// <returns>A new instance.</returns>
		static public SqlQuery CreateQuery(ISqlQuery query)
		{
			return CreateQuery(query.QueryString);
		}
		
		/// <summary>
		/// Set a single named parameter.
		/// </summary>
		/// <param name="name">The named parameter.</param>
		/// <param name="value">The value.</param>
		/// <returns>This instance.</returns>
		public SqlQuery SetParameter(string name, object value)
		{
			return SetParameter(name, value, Formatter);
		}

		/// <summary>
		/// Set a single named parameter.
		/// </summary>
		/// <param name="name">The named parameter.</param>
		/// <param name="value">The value.</param>
		/// <param name="formatter">The parameter formatter.</param>
		/// <returns>This instance.</returns>
		public SqlQuery SetParameter(string name, object value, ISqlFormatter formatter)
		{
			if (String.IsNullOrEmpty(name))
				throw new ArgumentNullException("name");

			if (formatter == null)
				throw new ArgumentNullException("formatter");

			this.QueryString = Replace(name, formatter.GetValue(value));

			return this;
		}

		/// <summary>
		/// Set a single named parameter.
		/// </summary>
		/// <typeparam name="TSource">The type of value.</typeparam>
		/// <param name="name">The named parameter.</param>
		/// <param name="value">The value.</param>
		/// <returns>The instance.</returns>
		public SqlQuery SetParameter<TSource>(string name, TSource value)
		{
			return SetParameter<TSource>(name, value, Formatter);
		}

		/// <summary>
		/// Set a single named parameter.
		/// </summary>
		/// <typeparam name="TSource">The type of value.</typeparam>
		/// <param name="name">The named parameter.</param>
		/// <param name="value">The value.</param>
		/// <param name="formatter">The value formatter.</param>
		/// <returns>This instance.</returns>
		public SqlQuery SetParameter<TSource>(string name, TSource value, ISqlFormatter formatter)
		{
			if (String.IsNullOrEmpty(name))
				throw new ArgumentNullException("name");

			if (formatter == null)
				throw new ArgumentNullException("formatter");

			this.QueryString = Replace(name, formatter.GetValue(value));

			return this;
		}

		/// <summary>
		/// Set a single named parameter.
		/// </summary>
		/// <param name="name">The named parameter</param>
		/// <param name="value">The value</param>
		/// <returns>This instance.</returns>
		public SqlQuery SetParameter(string name, string value)
		{
			return SetParameter(name, value, Formatter);
		}

		/// <summary>
		/// Set a single named parameter.
		/// </summary>
		/// <param name="name">The named parameter.</param>
		/// <param name="value">The value.</param>
		/// <param name="formatter">The value formatter.</param>
		/// <returns>This instance.</returns>
		public SqlQuery SetParameter(string name, string value, ISqlFormatter formatter)
		{
			if (String.IsNullOrEmpty(name))
				throw new ArgumentNullException("name");

			if (formatter == null)
				throw new ArgumentNullException("formatter");

			this.QueryString = Replace(name, formatter.GetValue(value));

			return this;
		}

		/// <summary>
		/// Set a single named parameter.
		/// </summary>
		/// <param name="name">The named parameter.</param>
		/// <param name="value">The value.</param>
		/// <returns>This instance.</returns>
		public SqlQuery SetParameter(string name, int value)
		{
			return SetParameter(name, value, Formatter);
		}

		/// <summary>
		/// Set a single named parameter.
		/// </summary>
		/// <param name="name">The named parameter.</param>
		/// <param name="value">The value.</param>
		/// <param name="formatter">The value formatter.</param>
		/// <returns>This instance.</returns>
		public SqlQuery SetParameter(string name, int value, ISqlFormatter formatter)
		{
			if (String.IsNullOrEmpty(name))
				throw new ArgumentNullException("name");

			if (formatter == null)
				throw new ArgumentNullException("formatter");

			this.QueryString = Replace(name, formatter.GetValue(value));

			return this;
		}

		/// <summary>
		/// Set a single named parameter.
		/// </summary>
		/// <param name="name">The named parameter.</param>
		/// <param name="value">The value.</param>
		/// <returns>This instance.</returns>
		public SqlQuery SetParameter(string name, long value)
		{
			return SetParameter(name, value, Formatter);
		}

		/// <summary>
		/// Set a single named parameter.
		/// </summary>
		/// <param name="name">The named parameter.</param>
		/// <param name="value">The value.</param>
		/// <param name="formatter">The value formatter.</param>
		/// <returns></returns>
		public SqlQuery SetParameter(string name, long value, ISqlFormatter formatter)
		{
			if (String.IsNullOrEmpty(name))
				throw new ArgumentNullException("name");

			if (formatter == null)
				throw new ArgumentNullException("formatter");

			this.QueryString = Replace(name, formatter.GetValue(value));

			return this;
		}

		/// <summary>
		/// Set a single named parameter.
		/// </summary>
		/// <param name="name">The named parameter.</param>
		/// <param name="value">The value.</param>
		/// <returns>This instance.</returns>
		public SqlQuery SetParameter(string name, float value)
		{
			return SetParameter(name, value, Formatter);
		}

		/// <summary>
		/// Set a single named parameter.
		/// </summary>
		/// <param name="name">The named parameter.</param>
		/// <param name="value">The value.</param>
		/// <param name="formatter">The value formatter.</param>
		/// <returns>This instance.</returns>
		public SqlQuery SetParameter(string name, float value, ISqlFormatter formatter)
		{
			if (String.IsNullOrEmpty(name))
				throw new ArgumentNullException("name");

			if (formatter == null)
				throw new ArgumentNullException("formatter");

			this.QueryString = Replace(name, formatter.GetValue(value));

			return this;
		}

		/// <summary>
		/// Set a single named parameter.
		/// </summary>
		/// <param name="name">The named parameter.</param>
		/// <param name="value">The value.</param>
		/// <returns>This instance.</returns>
		public SqlQuery SetParameter(string name, double value)
		{
			return SetParameter(name, value, Formatter);
		}

		/// <summary>
		/// Set a single named parameter.
		/// </summary>
		/// <param name="name">The named parameter.</param>
		/// <param name="value">The value.</param>
		/// <param name="formatter">The value formatter.</param>
		/// <returns>This instance.</returns>
		public SqlQuery SetParameter(string name, double value, ISqlFormatter formatter)
		{
			if (String.IsNullOrEmpty(name))
				throw new ArgumentNullException("name");

			if (formatter == null)
				throw new ArgumentNullException("formatter");

			this.QueryString = Replace(name, formatter.GetValue(value));

			return this;
		}

		/// <summary>
		/// Set a single named parameter.
		/// </summary>
		/// <param name="name">The named parameter.</param>
		/// <param name="value">The value.</param>
		/// <returns>This instance.</returns>
		public SqlQuery SetParameter(string name, decimal value)
		{
			return SetParameter(name, value, Formatter);
		}

		/// <summary>
		/// Set a single named parameter.
		/// </summary>
		/// <param name="name">The named paramter.</param>
		/// <param name="value">The value.</param>
		/// <param name="formatter">The value formatter.</param>
		/// <returns>This instance.</returns>
		public SqlQuery SetParameter(string name, decimal value, ISqlFormatter formatter)
		{
			if (String.IsNullOrEmpty(name))
				throw new ArgumentNullException("name");

			if (formatter == null)
				throw new ArgumentNullException("formatter");

			this.QueryString = Replace(name, formatter.GetValue(value));

			return this;
		}

		/// <summary>
		/// Set a single named parameter.
		/// </summary>
		/// <param name="name">The named parameter.</param>
		/// <param name="value">The value.</param>
		/// <returns>This instance.</returns>
		public SqlQuery SetParameter(string name, char value)
		{
			return SetParameter(name, value, Formatter);
		}

		/// <summary>
		/// Set a single named parameter.
		/// </summary>
		/// <param name="name">The named parameter.</param>
		/// <param name="value">The value.</param>
		/// <param name="formatter">The value formatter.</param>
		/// <returns>This instance.</returns>
		public SqlQuery SetParameter(string name, char value, ISqlFormatter formatter)
		{
			if (String.IsNullOrEmpty(name))
				throw new ArgumentNullException("name");

			if (formatter == null)
				throw new ArgumentNullException("formatter");

			this.QueryString = Replace(name, formatter.GetValue(value));

			return this;
		}
		
		/// <summary>
		/// Set a single named parameter.
		/// </summary>
		/// <param name="name">The named parameter.</param>
		/// <param name="value">The value.</param>
		/// <returns>This instance.</returns>
		public SqlQuery SetParameter(string name, DateTime value)
		{
			return SetParameter(name, value, Formatter);
		}

		/// <summary>
		/// Set a single named parameter.
		/// </summary>
		/// <param name="name">The named parameter.</param>
		/// <param name="value">The value.</param>
		/// <param name="formatter">The value formatter.</param>
		/// <returns>This instance.</returns>
		public SqlQuery SetParameter(string name, DateTime value, ISqlFormatter formatter)
		{
			if (String.IsNullOrEmpty(name))
				throw new ArgumentNullException("name");

			if (formatter == null)
				throw new ArgumentNullException("formatter");

			this.QueryString = Replace(name, formatter.GetValue(value));

			return this;
		}

		/// <summary>
		/// Set a single named parameter.
		/// </summary>
		/// <param name="name">The named parameter.</param>
		/// <param name="value">The value.</param>
		/// <returns>The instance.</returns>
		public SqlQuery SetParameter(string name, Guid value)
		{
			return SetParameter(name, value, Formatter);
		}

		/// <summary>
		/// Set a single named parameter.
		/// </summary>
		/// <param name="name">The named parameter.</param>
		/// <param name="value">The value.</param>
		/// <param name="formatter">The value formatter.</param>
		/// <returns></returns>
		public SqlQuery SetParameter(string name, Guid value, ISqlFormatter formatter)
		{
			if (String.IsNullOrEmpty(name))
				throw new ArgumentNullException("name");

			if (formatter == null)
				throw new ArgumentNullException("formatter");

			this.QueryString = Replace(name, formatter.GetValue(value));

			return this;
		}

		
		/// <summary>
		/// Set a single named parameter.
		/// </summary>
		/// <param name="name">The named parameter.</param>
		/// <param name="value">The value.</param>
		/// <returns>This instance.</returns>
		public SqlQuery SetParameter(string name, byte[] value)
		{
			return SetParameter(name, value, Formatter);
		}

		/// <summary>
		/// Set a single named parameter.
		/// </summary>
		/// <param name="name">The named parameter.</param>
		/// <param name="value">The value.</param>
		/// <param name="formatter">The value formatter.</param>
		/// <returns>This instance.</returns>
		public SqlQuery SetParameter(string name, byte[] value, ISqlFormatter formatter)
		{
			if (String.IsNullOrEmpty(name))
				throw new ArgumentNullException("name");

			if (value == null || value.Length == 0)
				throw new ArgumentException("The value can't be NULL or empty.");

			if (formatter == null)
				throw new ArgumentNullException("formatter");

			this.QueryString = Replace(name, formatter.GetValue(value));

			return this;
		}
		
		/// <summary>
		/// Add sql query object and return a new instance.
		/// </summary>
		/// <param name="query">A sql object contains the query.</param>
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
		/// <param name="query">A query or a fragment of a query.</param>
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
		/// <param name="query">A sql object contains the query.</param>
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
		/// <param name="query">A query or a fragment of a query.</param>
		/// <returns>This instance</returns>
		public SqlQuery Append(string query)
		{
			if (String.IsNullOrEmpty(query))
				throw new ArgumentNullException("query");

			this.QueryString = String.Format("{0} {1}", this.QueryString, query);
			return this;
		}
		
		/// <summary>
		/// The sql query string.
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
		/// <returns>The replaced sql query</returns>
		private string Replace(string name, string value)
		{
			if (String.IsNullOrEmpty(name))
				throw new ArgumentNullException("name");

			return RegexParameter.Replace(this.QueryString, match =>
			{
				var key = match.Groups[1].Value;
				if (key.Equals(name, StringComparison.OrdinalIgnoreCase))
				{
					return match.Result(value);
				}
				return match.Value;
			});
		}

		#endregion
	}
}
