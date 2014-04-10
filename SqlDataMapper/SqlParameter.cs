using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections.Specialized;

namespace SqlDataMapper
{
	/// <summary>
	/// A class to provide parameter for sql statements.
	/// </summary>
	public class SqlParameter : HybridDictionary
	{
		/// <summary>
		/// Create a new instance.
		/// </summary>
		public SqlParameter()
		{ }

		/// <summary>
		/// Create a new instance and add a single parameter.
		/// </summary>
		/// <param name="key">A unique key.</param>
		/// <param name="value">A value.</param>
		public SqlParameter(string key, object value)
		{
			if (String.IsNullOrEmpty(key))
				throw new ArgumentNullException("key");

			base.Add(key, value);
		}

		/// <summary>
		/// Returns if this instance has parameters.
		/// </summary>
		public bool HasParameters
		{
			get
			{
				if (this.Count > 0)
					return true;
				else
					return false;
			}
		}

		/// <summary>
		/// Add new parameters.
		/// </summary>
		/// <param name="key">A unique key.</param>
		/// <param name="value">The value.</param>
		public void Add(string key, object value)
		{
			if(String.IsNullOrEmpty(key))
				throw new ArgumentNullException("key");
			
			if (!this.Contains(key))
			{
				base.Add(key, value);
			}
		}

		/// <summary>
		/// Get or set a parameter.
		/// </summary>
		/// <param name="key">A unique key.</param>
		/// <returns>The value.</returns>
		public object this[string key]
		{
			get
			{
				return base[key];
			}
			set
			{
				base[key] = value;
			}
		}
	}
}
