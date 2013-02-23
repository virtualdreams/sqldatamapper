using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SqlDataMapper
{
	/// <summary>
	/// Interface for query objects
	/// </summary>
	public interface ISqlQuery
	{
		/// <summary>
		/// Get the sql query
		/// </summary>
		string QueryString { get; }
		
		/// <summary>
		/// Check for unresolved parameters
		/// </summary>
		/// <param name="check">Do the check?</param>
		/// <returns>This instance</returns>
		ISqlQuery Check(bool check);
		
		/// <summary>
		/// Replaces the whole existing query with the new query
		/// </summary>
		/// <param name="query">The new query</param>
		/// <returns>This instance</returns>
		ISqlQuery Set(string query);
	}
}
