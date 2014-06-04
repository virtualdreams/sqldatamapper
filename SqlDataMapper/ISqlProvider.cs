using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SqlDataMapper
{
	/// <summary>
	/// Generic interface for sql provider
	/// </summary>
	public interface ISqlProvider
	{
		/// <summary>
		/// Must provide open functionality
		/// </summary>
		void Open();
		
		/// <summary>
		/// Must provide close functionality
		/// </summary>
		void Close();
		
		/// <summary>
		/// Must provide begin transaction functionality
		/// </summary>
		void BeginTransaction();
		
		/// <summary>
		/// Must provide commit transaction functionality
		/// </summary>
		void CommitTransaction();
		
		/// <summary>
		/// Must provide rollback transaction functionality
		/// </summary>
		void RollbackTransaction();
		
		/// <summary>
		/// Must provide select object functionality
		/// </summary>
		T Select<T>(string query);
		
		/// <summary>
		/// Must provide select list functionality
		/// </summary>
		T[] SelectList<T>(string query);
		
		/// <summary>
		/// Must provide select scalar functionality
		/// </summary>
		T SelectScalar<T>(string query);

		/// <summary>
		/// Must provide select list functionality, implemented as scalar list.
		/// </summary>
		T[] SelectScalarList<T>(string query);
		
		/// <summary>
		/// Must provide insert functionality
		/// </summary>
		int Insert(string query);
		
		/// <summary>
		/// Must provide update functionality
		/// </summary>
		int Update(string query);
		
		/// <summary>
		/// Must provide delete functionality
		/// </summary>
		int Delete(string query);
	}
}
