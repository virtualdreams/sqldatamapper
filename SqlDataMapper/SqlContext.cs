using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SqlDataMapper
{
	/// <summary>
	/// The sql context
	/// </summary>
	public class SqlContext
	{
		private ISqlProvider m_Provider = null;
		private bool m_IsTransactionSession = false;
		private bool m_ParameterCheck = false;
		
		#region Constructors
		/// <summary>
		/// Create a new context.
		/// </summary>
		/// <param name="assemblyName"></param>
		/// <param name="connectionClass"></param>
		/// <param name="connectionString"></param>
		public SqlContext(string assemblyName, string connectionClass, string connectionString)
		{
			if (String.IsNullOrEmpty(assemblyName))
				throw new ArgumentNullException("assemblyName");

			if (String.IsNullOrEmpty(connectionClass))
				throw new ArgumentNullException("connectionClass");

			if (String.IsNullOrEmpty(connectionString))
				throw new ArgumentNullException("connectionString");
			
			m_Provider = new SqlProvider(assemblyName, connectionClass, connectionString);
		}
		
		/// <summary>
		/// Create a new context out of a custom provider
		/// </summary>
		/// <param name="provider"></param>
		public SqlContext(ISqlProvider provider)
		{
			if (provider == null)
				throw new ArgumentNullException("provider");
			
			m_Provider = provider;
		}
		#endregion
		
		#region member
		/// <summary>
		/// Enable or disable parameter check after replace placeholders
		/// </summary>
		public bool ParameterCheck
		{
			get
			{
				return m_ParameterCheck;
			}
			set
			{
				m_ParameterCheck = value;
			}
		}
		#endregion

		#region sql methods

		/// <summary>
		/// Begins a database transaction.
		/// </summary>
		/// <remarks>
		/// The method opens a connection to the datebase. The connection will closed through <c>CommitTransaction</c> or <c>RollbackTransaction</c>.
		/// </remarks>
		public void BeginTransaction()
		{
			if (m_IsTransactionSession)
			{
				throw new SqlDataMapperException("SqlMapper could not invoke BeginTransaction(). A transaction is already started. Call CommitTransaction() or RollbackTransaction() first.");
			}

			try
			{
				m_Provider.Open();
				m_IsTransactionSession = true;
				m_Provider.BeginTransaction();
			}
			catch (Exception ex)
			{
				m_Provider.Close();
				m_IsTransactionSession = false;
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
			if (!m_IsTransactionSession)
			{
				throw new SqlDataMapperException("SqlMapper could not invoke CommitTransaction(). No transaction was started. Call BeginTransaction() first.");
			}

			try
			{
				m_Provider.CommitTransaction();
			}
			catch (Exception ex)
			{
				try
				{
					m_Provider.RollbackTransaction();
				}
				catch (Exception iex)
				{
					throw iex;
				}
				throw ex;
			}
			finally
			{
				m_IsTransactionSession = false;
				m_Provider.Close();
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
			if (!m_IsTransactionSession)
			{
				throw new SqlDataMapperException("SqlMapper could not invoke CommitTransaction(). No transaction was started. Call BeginTransaction() first.");
			}

			try
			{
				m_Provider.RollbackTransaction();
			}
			catch (Exception ex)
			{
				throw ex;
			}
			finally
			{
				m_IsTransactionSession = false;
				m_Provider.Close();
			}
		}

		/// <summary>
		/// Executes a sql select statement that returns data to populate a single object instance.
		/// </summary>
		/// <typeparam name="T">The object type</typeparam>
		/// <param name="query">The query object</param>
		/// <returns>A single object</returns>
		public T QueryForObject<T>(ISqlQuery query)
		{
			bool flag = false;
			try
			{
				if (!m_IsTransactionSession)
				{
					m_Provider.Open();
					flag = true;
				}

				return m_Provider.Select<T>(query.Check(this.ParameterCheck).QueryString);
			}
			catch (Exception ex)
			{
				throw ex;
			}
			finally
			{
				if (flag)
				{
					m_Provider.Close();
				}
			}
		}

		/// <summary>
		/// Executes a sql select statement that returns data to populate a number of result objects.
		/// </summary>
		/// <typeparam name="TDestination">The object type</typeparam>
		/// <param name="query">The query object</param>
		/// <returns>A list ob objects</returns>
		public TDestination[] QueryForList<TDestination>(ISqlQuery query)
		{
			bool flag = false;
			try
			{
				if (!m_IsTransactionSession)
				{
					m_Provider.Open();
					flag = true;
				}

				return m_Provider.SelectList<TDestination>(query.Check(this.ParameterCheck).QueryString);
			}
			catch (Exception ex)
			{
				throw ex;
			}
			finally
			{
				if (flag)
				{
					m_Provider.Close();
				}
			}
		}

		/// <summary>
		/// Executes a sql select statement that returns a single value.
		/// </summary>
		/// <typeparam name="T">The object</typeparam>
		/// <param name="query">The query object</param>
		/// <returns>A single object</returns>
		public T QueryForScalar<T>(ISqlQuery query)
		{
			bool flag = false;
			try
			{
				if (!m_IsTransactionSession)
				{
					m_Provider.Open();
					flag = true;
				}

				return m_Provider.SelectScalar<T>(query.Check(this.ParameterCheck).QueryString);
			}
			catch (Exception ex)
			{
				throw ex;
			}
			finally
			{
				if (flag)
				{
					m_Provider.Close();
				}
			}
		}

		/// <summary>
		/// Executes a sql statement that returns a list of single values.
		/// </summary>
		/// <typeparam name="TDestination">The object</typeparam>
		/// <param name="query">The query object</param>
		/// <returns>A list of single objects</returns>
		public TDestination[] QueryForScalarList<TDestination>(ISqlQuery query)
		{
			bool flag = false;
			try
			{
				if (!m_IsTransactionSession)
				{
					m_Provider.Open();
					flag = true;
				}

				return m_Provider.SelectScalarList<TDestination>(query.Check(this.ParameterCheck).QueryString);
			}
			catch (Exception ex)
			{
				throw ex;
			}
			finally
			{
				if (flag)
				{
					m_Provider.Close();
				}
			}
		}

		/// <summary>
		/// Executes a sql insert statement.
		/// </summary>
		/// <param name="query">The query object</param>
		/// <returns>The affected rows</returns>
		public int Insert(ISqlQuery query)
		{
			bool flag = false;
			try
			{
				if (!m_IsTransactionSession)
				{
					m_Provider.Open();
					flag = true;
				}

				return m_Provider.Insert(query.Check(this.ParameterCheck).QueryString);
			}
			catch (Exception ex)
			{
				throw ex;
			}
			finally
			{
				if (flag)
				{
					m_Provider.Close();
				}
			}
		}

		/// <summary>
		/// Executes a sql update statement.
		/// </summary>
		/// <param name="query">The query object</param>
		/// <returns>The affected rows</returns>
		public int Update(ISqlQuery query)
		{
			bool flag = false;
			try
			{
				if (!m_IsTransactionSession)
				{
					m_Provider.Open();
					flag = true;
				}

				return m_Provider.Update(query.Check(this.ParameterCheck).QueryString);
			}
			catch (Exception ex)
			{
				throw ex;
			}
			finally
			{
				if (flag)
				{
					m_Provider.Close();
				}
			}
		}

		/// <summary>
		/// Executes a sql delete statement.
		/// </summary>
		/// <param name="query">The query object</param>
		/// <returns>The affected rows</returns>
		public int Delete(ISqlQuery query)
		{
			bool flag = false;
			try
			{
				if (!m_IsTransactionSession)
				{
					m_Provider.Open();
					flag = true;
				}

				return m_Provider.Delete(query.Check(this.ParameterCheck).QueryString);
			}
			catch (Exception ex)
			{
				throw ex;
			}
			finally
			{
				if (flag)
				{
					m_Provider.Close();
				}
			}
		}

		#endregion
	}
}
