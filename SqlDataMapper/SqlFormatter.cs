using System;
using System.Collections;
using System.Text;
using SqlDataMapper.Extension;

namespace SqlDataMapper
{
	/// <summary>
	/// Default value formatter.
	/// </summary>
	internal sealed class SqlFormatter: ISqlFormatter
	{
		/// <summary>
		/// Convert the the value to a sql type. Enumerables, excluding strings and byte-arrays, will transformed to comma separated values.
		/// </summary>
		/// <param name="value">The value.</param>
		/// <returns>A sql compatible string.</returns>
		public object GetValue(object value)
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
				return String.Format("0x{0}", (value as byte[]).ToHex());
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
	}
}
