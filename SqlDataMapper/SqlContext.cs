using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SqlDataMapper
{
	/// <summary>
	/// The sql context
	/// </summary>
	public class SqlContext: ISqlContext
	{
		private ISqlProvider Provider { get; set; }
		private bool IsTransactionSession { get; set; }
		private bool ParameterCheck { get; set; }
		
		/// <summary>
		/// Create a new context out of a custom provider
		/// </summary>
		/// <param name="provider"></param>
		public SqlContext(ISqlProvider provider)
		{
			if (provider == null)
				throw new ArgumentNullException("provider");
			
			Provider = provider;
		}

		/// <summary>
		/// Begins a database transaction.
		/// </summary>
		/// <remarks>
		/// The method opens a connection to the datebase. The connection will closed through <c>CommitTransaction</c> or <c>RollbackTransaction</c>.
		/// </remarks>
		public void BeginTransaction()
		{
			if (IsTransactionSession)
			{
				throw new SqlDataMapperException("SqlMapper could not invoke BeginTransaction(). A transaction is already started. Call CommitTransaction() or RollbackTransaction() first.");
			}

			try
			{
				Provider.Open();
				IsTransactionSession = true;
				Provider.BeginTransaction();
			}
			catch (Exception ex)
			{
				Provider.Close();
				IsTransactionSession = false;
				throw ex;
			}
		}

		/// <summary>
		/// Commits the database transaction.
		/// </summary>
		/// <remarks>
		/// The connection will closed.
		/// </remarks>
		public void CommitTransaction()
		{
			if (!IsTransactionSession)
			{
				throw new SqlDataMapperException("SqlMapper could not invoke CommitTransaction(). No transaction was started. Call BeginTransaction() first.");
			}

			try
			{
				Provider.CommitTransaction();
			}
			catch (Exception ex)
			{
				try
				{
					Provider.RollbackTransaction();
				}
				catch (Exception iex)
				{
					throw iex;
				}
				throw ex;
			}
			finally
			{
				IsTransactionSession = false;
				Provider.Close();
			}
		}

		/// <summary>
		/// Rolls back a transaction from a pending state.
		/// </summary>
		/// <remarks>
		/// The connection will closed.
		/// </remarks>
		public void RollbackTransaction()
		{
			if (!IsTransactionSession)
			{
				throw new SqlDataMapperException("SqlMapper could not invoke RollbackTransaction(). No transaction was started. Call BeginTransaction() first.");
			}

			try
			{
				Provider.RollbackTransaction();
			}
			catch (Exception ex)
			{
				throw ex;
			}
			finally
			{
				IsTransactionSession = false;
				Provider.Close();
			}
		}

		/// <summary>
		/// Executes a sql select statement that returns a single data object.
		/// </summary>
		/// <typeparam name="TDestination">The destination type.</typeparam>
		/// <param name="query">The query.</param>
		/// <returns>The data object.</returns>
		public TDestination QueryForObject<TDestination>(ISqlQuery query) where TDestination : class, new()
		{
			bool closeConnection = false;
			try
			{
				if (!IsTransactionSession)
				{
					Provider.Open();
					closeConnection = true;
				}

				return Provider.SelectObject<TDestination>(query.Check(this.ParameterCheck).QueryString);
			}
			catch (Exception ex)
			{
				throw ex;
			}
			finally
			{
				if (closeConnection)
				{
					Provider.Close();
				}
			}
		}

		/// <summary>
		/// Executes a sql select statement that returns a list of data objects.
		/// </summary>
		/// <typeparam name="TDestination">The destination type.</typeparam>
		/// <param name="query">The query.</param>
		/// <returns>The list of data objects</returns>
		public IEnumerable<TDestination> QueryForObjectList<TDestination>(ISqlQuery query) where TDestination : class, new()
		{
			bool closeConnection = false;
			try
			{
				if (!IsTransactionSession)
				{
					Provider.Open();
					closeConnection = true;
				}

				return Provider.SelectObjectList<TDestination>(query.Check(this.ParameterCheck).QueryString).ToArray();
			}
			catch (Exception ex)
			{
				throw ex;
			}
			finally
			{
				if (closeConnection)
				{
					Provider.Close();
				}
			}
		}

		/// <summary>
		/// Executes a sql select statement that returns a single value.
		/// </summary>
		/// <typeparam name="TDestination">The destination type.</typeparam>
		/// <param name="query">The query.</param>
		/// <returns>The value.</returns>
		public TDestination QueryForScalar<TDestination>(ISqlQuery query) where TDestination : IConvertible
		{
			bool closeConnection = false;
			try
			{
				if (!IsTransactionSession)
				{
					Provider.Open();
					closeConnection = true;
				}

				return Provider.SelectScalar<TDestination>(query.Check(this.ParameterCheck).QueryString);
			}
			catch (Exception ex)
			{
				throw ex;
			}
			finally
			{
				if (closeConnection)
				{
					Provider.Close();
				}
			}
		}

		/// <summary>
		/// Executes a sql statement that returns a list of single values.
		/// </summary>
		/// <typeparam name="TDestination">The destination type.</typeparam>
		/// <param name="query">The query.</param>
		/// <returns>A list of values.</returns>
		public IEnumerable<TDestination> QueryForScalarList<TDestination>(ISqlQuery query) where TDestination : IConvertible
		{
			bool closeConnection = false;
			try
			{
				if (!IsTransactionSession)
				{
					Provider.Open();
					closeConnection = true;
				}

				return Provider.SelectScalarList<TDestination>(query.Check(this.ParameterCheck).QueryString).ToArray();
			}
			catch (Exception ex)
			{
				throw ex;
			}
			finally
			{
				if (closeConnection)
				{
					Provider.Close();
				}
			}
		}

		/// <summary>
		/// Executes a sql insert statement.
		/// </summary>
		/// <param name="query">The query.</param>
		/// <returns>The affected rows</returns>
		public int Insert(ISqlQuery query)
		{
			bool flag = false;
			try
			{
				if (!IsTransactionSession)
				{
					Provider.Open();
					flag = true;
				}

				return Provider.Insert(query.Check(this.ParameterCheck).QueryString);
			}
			catch (Exception ex)
			{
				throw ex;
			}
			finally
			{
				if (flag)
				{
					Provider.Close();
				}
			}
		}

		/// <summary>
		/// Executes a sql update statement.
		/// </summary>
		/// <param name="query">The query.</param>
		/// <returns>The affected rows</returns>
		public int Update(ISqlQuery query)
		{
			bool flag = false;
			try
			{
				if (!IsTransactionSession)
				{
					Provider.Open();
					flag = true;
				}

				return Provider.Update(query.Check(this.ParameterCheck).QueryString);
			}
			catch (Exception ex)
			{
				throw ex;
			}
			finally
			{
				if (flag)
				{
					Provider.Close();
				}
			}
		}

		/// <summary>
		/// Executes a sql delete statement.
		/// </summary>
		/// <param name="query">The query.</param>
		/// <returns>The affected rows</returns>
		public int Delete(ISqlQuery query)
		{
			bool flag = false;
			try
			{
				if (!IsTransactionSession)
				{
					Provider.Open();
					flag = true;
				}

				return Provider.Delete(query.Check(this.ParameterCheck).QueryString);
			}
			catch (Exception ex)
			{
				throw ex;
			}
			finally
			{
				if (flag)
				{
					Provider.Close();
				}
			}
		}
	}
}
