using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Runtime.Serialization;

namespace SqlDataMapper
{
	/// <summary>
	/// This exception is thrown when an error in the SqlMapper occurs.
	/// </summary>
	/// <remarks>
	/// This is the base exception for all exceptions thrown in the SqlMapper
	/// </remarks>
	[Serializable]
	public class SqlDataMapperException: ApplicationException
	{
		/// <summary>
		/// Initializes a new instance og the <see cref="T:SqlDataMapper.SqlDataMapperException" /> class.
		/// </summary>
		public SqlDataMapperException() 
			: base("SqlDataMapper caused an exception.")
		{
		}

		/// <summary>
		/// Initializes a new instance og the <see cref="T:SqlDataMapper.SqlDataMapperException" /> class.
		/// </summary>
		public SqlDataMapperException(Exception ex)
			: base("SqlDataMapper caused an exception.", ex)
		{
		}

		/// <summary>
		/// Initializes a new instance og the <see cref="T:SqlDataMapper.SqlDataMapperException" /> class.
		/// </summary>
		public SqlDataMapperException(string message)
			: base(message)
		{
		}

		/// <summary>
		/// Initializes a new instance og the <see cref="T:SqlDataMapper.SqlDataMapperException" /> class.
		/// </summary>
		public SqlDataMapperException(string message, Exception innerException) 
			: base(message, innerException)
		{
		}

		/// <summary>
		/// Initializes a new instance og the <see cref="T:SqlDataMapper.SqlDataMapperException" /> class.
		/// </summary>
		protected SqlDataMapperException(SerializationInfo info, StreamingContext context)
			: base(info, context)
		{
		}
	}
}
