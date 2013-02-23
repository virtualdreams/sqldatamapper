using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections.Specialized;

namespace SqlDataMapper
{
	/// <summary>
	/// A simple class to provide parameter for sql statements.
	/// </summary>
	public class SqlParameter
	{
		private HybridDictionary m_Parameters = new HybridDictionary();
		
		/// <summary>
		/// Default constructor creates an empty parameter object
		/// </summary>
		public SqlParameter()
		{

		}
		
		/// <summary>
		/// Constructor for adding a single parameter at creation.
		/// </summary>
		/// <param name="key">A unique key</param>
		/// <param name="value">The value</param>
		public SqlParameter(string key, object value)
		{
			this.Add(key, value);
		}
		
		/// <summary>
		/// Get direct access to the dictionary of parameters
		/// </summary>
		internal HybridDictionary Parameters
		{
			get
			{
				return m_Parameters;
			}
		}
		
		/// <summary>
		/// Get the count of parameters.
		/// </summary>
		public int Count
		{
			get
			{
				return m_Parameters.Count;
			}
		}
		
		/// <summary>
		/// Return if there are parameters or not.
		/// </summary>
		public bool HasParameters
		{
			get
			{
				if(Count > 0)
					return true;
				else
					return false;
			}
		}

		/// <summary>
		/// Add new parameter and value.
		/// Existing parameters can't be overwritten.
		/// </summary>
		/// <param name="key">A unique key</param>
		/// <param name="value">The value</param>
		/// <returns>Returns true if key/value added</returns>
		public bool Add(string key, object value)
		{
			//TODO - check key definition
			if (!m_Parameters.Contains(key))
			{
				m_Parameters.Add(key, value);
				return true;
			}
			return false;
		}
		
		/// <summary>
		/// Get a parameter value.
		/// </summary>
		/// <param name="key">The unique key</param>
		/// <returns>The value</returns>
		public object Get(string key)
		{
			if (m_Parameters.Contains(key))
			{
				return m_Parameters[key];
			}
			return null;
		}

		/// <summary>
		/// Get or set a new parameter and value.
		/// Parameters can be overwritten.
		/// </summary>
		/// <param name="key"></param>
		/// <returns></returns>
		public object this[string key]
		{
			get
			{
				return this.Get(key);
			}
			set
			{
				if(!this.Add(key, value))
				{
					m_Parameters[key] = value;
				}
			}
		}
	}
}
