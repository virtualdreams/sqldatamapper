using System;
using System.Collections.Generic;

namespace SqlDataMapper
{
	/// <summary>
	/// Interface for sql context.
	/// </summary>
	public interface ISqlContext
	{
		/// <summary>
		/// Begins a database transaction.
		/// </summary>
		void BeginTransaction();

		/// <summary>
		/// Commit a pending database transaction.
		/// </summary>
		void CommitTransaction();

		/// <summary>
		/// Rollback a pending database transaction.
		/// </summary>
		void RollbackTransaction();

		/// <summary>
		/// Query for a single database object.
		/// </summary>
		/// <typeparam name="TDestination">The destination object.</typeparam>
		/// <param name="query">The query.</param>
		/// <returns>A single data object.</returns>
		TDestination QueryForObject<TDestination>(ISqlQuery query) where TDestination : class, new();

		/// <summary>
		/// Query for a list of database objects.
		/// </summary>
		/// <typeparam name="TDestination">The destination type.</typeparam>
		/// <param name="query">The query.</param>
		/// <returns>A list of database objects.</returns>
		IEnumerable<TDestination> QueryForObjectList<TDestination>(ISqlQuery query) where TDestination : class, new();

		/// <summary>
		/// Query for single value.
		/// </summary>
		/// <typeparam name="TDestination">The destination type.</typeparam>
		/// <param name="query">The query.</param>
		/// <returns>A single value.</returns>
		TDestination QueryForScalar<TDestination>(ISqlQuery query) where TDestination : IConvertible;
		
		/// <summary>
		/// Query for list of values.
		/// </summary>
		/// <typeparam name="TDestination">The destination type.</typeparam>
		/// <param name="query">The query.</param>
		/// <returns>A list of values.</returns>
		IEnumerable<TDestination> QueryForScalarList<TDestination>(ISqlQuery query) where TDestination : IConvertible;

		/// <summary>
		/// Statement to insert data.
		/// </summary>
		/// <param name="query">The query.</param>
		/// <returns>Affected rows.</returns>
		int Insert(ISqlQuery query);

		/// <summary>
		/// Statement to update data.
		/// </summary>
		/// <param name="query">The query.</param>
		/// <returns>Affected rows.</returns>
		int Update(ISqlQuery query);

		/// <summary>
		/// Statement to delete data.
		/// </summary>
		/// <param name="query">The query.</param>
		/// <returns>Affected rows.</returns>
		int Delete(ISqlQuery query);
	}
}
