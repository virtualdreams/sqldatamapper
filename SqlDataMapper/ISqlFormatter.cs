
namespace SqlDataMapper
{
	/// <summary>
	/// Interface for value formatter.
	/// </summary>
	public interface ISqlFormatter
	{
		/// <summary>
		/// Convert a value to a compatible sql string.
		/// </summary>
		/// <param name="value"></param>
		/// <returns></returns>
		string GetValue(object value);
	}
}
