
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
		/// Replaces the whole existing query with the new query
		/// </summary>
		/// <param name="query">The new query</param>
		/// <returns>This instance</returns>
		ISqlQuery Set(string query);
	}
}
