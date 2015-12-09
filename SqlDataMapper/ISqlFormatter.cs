using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

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
		object GetValue(object value);
	}
}
