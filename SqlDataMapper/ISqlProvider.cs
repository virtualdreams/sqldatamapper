using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SqlDataMapper
{
	/// <summary>
	/// Interface for sql provider
	/// </summary>
	public interface ISqlProvider
	{
		/// <summary>
		/// Provide open functionality
		/// </summary>
		void Open();
		
		/// <summary>
		/// Provide close functionality
		/// </summary>
		void Close();
		
		/// <summary>
		/// Provide begin transaction functionality
		/// </summary>
		void BeginTransaction();
		
		/// <summary>
		/// Provide commit transaction functionality
		/// </summary>
		void CommitTransaction();
		
		/// <summary>
		/// Provide rollback transaction functionality
		/// </summary>
		void RollbackTransaction();
		
		/// <summary>
		/// Provide select object functionality
		/// </summary>
		T SelectObject<T>(string query) where T : class, new();
		
		/// <summary>
		/// Provide select list functionality
		/// </summary>
		IEnumerable<T> SelectObjectList<T>(string query) where T : class, new();
		
		/// <summary>
		/// Provide select scalar functionality
		/// </summary>
		T SelectScalar<T>(string query) where T : IConvertible;

		/// <summary>
		/// Provide select list functionality, implemented as scalar list.
		/// </summary>
		IEnumerable<T> SelectScalarList<T>(string query) where T : IConvertible;
		
		/// <summary>
		/// Provide insert functionality
		/// </summary>
		int Insert(string query);
		
		/// <summary>
		/// Provide update functionality
		/// </summary>
		int Update(string query);
		
		/// <summary>
		/// Provide delete functionality
		/// </summary>
		int Delete(string query);
	}
}
