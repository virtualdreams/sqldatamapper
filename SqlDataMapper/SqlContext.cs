using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;

namespace SqlDataMapper
{
	/// <summary>
	/// This class handles the connection to the database. Results from
	/// the database will converted to classes or primitive types.
	/// </summary>
	public class SqlContext: ISqlContext
	{
		private bool IsInTransaction { get; set; }
		private IDbConnection Connection { get; set; }
		private IDbTransaction Transaction { get; set; }

		/// <summary>
		/// Create a new database context.
		/// </summary>
		/// <param name="assemblyName">The (full qualified) assembly name.</param>
		/// <param name="connectionClass">The connection class.</param>
		/// <param name="connectionString">The connection string.</param>
		public SqlContext(string assemblyName, string connectionClass, string connectionString)
		{
			try
			{
				Connection = (DbConnection)Activator.CreateInstance(assemblyName, connectionClass).Unwrap();
				Connection.ConnectionString = connectionString;
			}
			catch (Exception ex)
			{
				throw new SqlDataMapperException(String.Format("Can't create database object: {0}", ex.Message), ex);
			}
		}

		/// <summary>
		/// Open a connection to database.
		/// </summary>
		private void Open()
		{
			Close();
			try
			{
				Connection.Open();
			}
			catch (Exception ex)
			{
				Close();
				throw new SqlDataMapperException(String.Format("Can't open datenbase: {0}", ex.Message));
			}
		}

		/// <summary>
		/// Close the connection to database.
		/// </summary>
		private void Close()
		{
			if (Connection != null && Connection.State == ConnectionState.Open)
			{
				Connection.Close();
			}
		}

		/// <summary>
		/// Begins a database transaction.
		/// </summary>
		/// <remarks>
		/// The method opens a connection to the datebase. The connection will closed through <c>CommitTransaction</c> or <c>RollbackTransaction</c>.
		/// </remarks>
		public void BeginTransaction()
		{
			if (IsInTransaction)
			{
				throw new SqlDataMapperException("SqlContext could not invoke BeginTransaction(). A transaction is already started. Call CommitTransaction() or RollbackTransaction() first.");
			}

			try
			{
				Open();
				Transaction = Connection.BeginTransaction();
				IsInTransaction = true;
			}
			catch (Exception ex)
			{
				IsInTransaction = false;
				Close();
				
				throw ex;
			}
		}

		/// <summary>
		/// Begins a database transaction.
		/// </summary>
		/// <remarks>
		/// The method opens a connection to the datebase. The connection will closed through <c>CommitTransaction</c> or <c>RollbackTransaction</c>.
		/// </remarks>
		/// <param name="isolationLevel">Isolationlevel for the transaction.</param>
		public void BeginTransaction(IsolationLevel isolationLevel)
		{
			if (IsInTransaction)
			{
				throw new SqlDataMapperException("SqlContext could not invoke BeginTransaction(). A transaction is already started. Call CommitTransaction() or RollbackTransaction() first.");
			}

			try
			{
				Open();
				Transaction = Connection.BeginTransaction(isolationLevel);
				IsInTransaction = true;
			}
			catch (Exception ex)
			{
				IsInTransaction = false;
				Close();

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
			try
			{
				if (Transaction != null)
				{
					Transaction.Commit();
					Transaction.Dispose();
					Transaction = null;
				}
			}
			catch (Exception ex)
			{
				try
				{
					if (Transaction != null)
					{
						Transaction.Rollback();
						Transaction.Dispose();
						Transaction = null;
					}
				}
				catch (Exception iex)
				{
					throw iex;
				}
				throw ex;
			}
			finally
			{
				IsInTransaction = false;
				Close();
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
			try
			{
				if (Transaction != null)
				{
					Transaction.Rollback();
					Transaction.Dispose();
					Transaction = null;
				}
			}
			catch (Exception ex)
			{
				throw ex;
			}
			finally
			{
				IsInTransaction = false;
				Close();
			}
		}

		/// <summary>
		/// Run commands in a transaction.
		/// </summary>
		/// <param name="action"></param>
		public void RunInTransaction(Action action)
		{
			try
			{
				BeginTransaction();
				action();
				CommitTransaction();
			}
			catch (Exception)
			{
				RollbackTransaction();
				throw;
			}
		}

		/// <summary>
		/// Run commands in a transaction.
		/// </summary>
		/// <param name="isolationLevel">Isolationlevel for the transaction.</param>
		/// <param name="action"></param>
		public void RunInTransaction(IsolationLevel isolationLevel, Action action)
		{
			try
			{
				BeginTransaction(isolationLevel);
				action();
				CommitTransaction();
			}
			catch (Exception)
			{
				RollbackTransaction();
				throw;
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
				if (!IsInTransaction)
				{
					Open();
					closeConnection = true;
				}

				return QueryObject<TDestination>(query);
			}
			catch (Exception ex)
			{
				throw ex;
			}
			finally
			{
				if (closeConnection)
				{
					Close();
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
				if (!IsInTransaction)
				{
					Open();
					closeConnection = true;
				}

				return QueryObjectList<TDestination>(query);
			}
			catch (Exception ex)
			{
				throw ex;
			}
			finally
			{
				if (closeConnection)
				{
					Close();
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
				if (!IsInTransaction)
				{
					Open();
					closeConnection = true;
				}

				return QueryScalar<TDestination>(query);
			}
			catch (Exception ex)
			{
				throw ex;
			}
			finally
			{
				if (closeConnection)
				{
					Close();
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
				if (!IsInTransaction)
				{
					Open();
					closeConnection = true;
				}

				return QueryScalarList<TDestination>(query);
			}
			catch (Exception ex)
			{
				throw ex;
			}
			finally
			{
				if (closeConnection)
				{
					Close();
				}
			}
		}

		/// <summary>
		/// Executes a sql insert statement.
		/// </summary>
		/// <param name="query">The query.</param>
		/// <returns>The affected rows.</returns>
		public int Insert(ISqlQuery query)
		{
			bool closeConnection = false;
			try
			{
				if (!IsInTransaction)
				{
					Open();
					closeConnection = true;
				}

				return ExecuteNonQuery(query);
			}
			catch (Exception ex)
			{
				throw ex;
			}
			finally
			{
				if (closeConnection)
				{
					Close();
				}
			}
		}

		/// <summary>
		/// Executes a sql update statement.
		/// </summary>
		/// <param name="query">The query.</param>
		/// <returns>The affected rows.</returns>
		public int Update(ISqlQuery query)
		{
			bool closeConnection = false;
			try
			{
				if (!IsInTransaction)
				{
					Open();
					closeConnection = true;
				}

				return ExecuteNonQuery(query);
			}
			catch (Exception ex)
			{
				throw ex;
			}
			finally
			{
				if (closeConnection)
				{
					Close();
				}
			}
		}

		/// <summary>
		/// Executes a sql delete statement.
		/// </summary>
		/// <param name="query">The query.</param>
		/// <returns>The affected rows.</returns>
		public int Delete(ISqlQuery query)
		{
			bool closeConnection = false;
			try
			{
				if (!IsInTransaction)
				{
					Open();
					closeConnection = true;
				}

				return ExecuteNonQuery(query);
			}
			catch (Exception ex)
			{
				throw ex;
			}
			finally
			{
				if (closeConnection)
				{
					Close();
				}
			}
		}

		/// <summary>
		/// Query for scalar list.
		/// </summary>
		/// <typeparam name="TDestination">The destination type.</typeparam>
		/// <param name="query">The query.</param>
		/// <returns>List of scalar values.</returns>
		private IEnumerable<TDestination> QueryScalarList<TDestination>(ISqlQuery query) where TDestination : IConvertible
		{
			using (IDbCommand cmd = Connection.CreateCommand())
			{
				cmd.CommandText = query.QueryString;

				if (IsInTransaction && Transaction != null)
				{
					cmd.Transaction = Transaction;
				}

				cmd.Prepare();

				using (IDataReader reader = cmd.ExecuteReader())
				{
					List<TDestination> result = new List<TDestination>();
					while (reader.Read())
					{
						object obj = reader.GetValue(0);

						if (obj is TDestination)
						{
							result.Add((TDestination)obj);
						}
						else
						{
							TDestination _return = default(TDestination);
							try
							{
								_return = (TDestination)Convert.ChangeType(obj, typeof(TDestination));
							}
							catch (Exception ex)
							{
								throw new SqlDataMapperException(String.Format("Invalid cast. Type '{0}' is required.", obj.GetType()), ex);
							}
							result.Add(_return);
						}
					}
					return result;
				}
			}
		}

		/// <summary>
		/// Query scalar.
		/// </summary>
		/// <typeparam name="TDestination">The destination type.</typeparam>
		/// <param name="query">The query.</param>
		/// <returns>The scalar value.</returns>
		private TDestination QueryScalar<TDestination>(ISqlQuery query) where TDestination : IConvertible
		{
			using (IDbCommand cmd = Connection.CreateCommand())
			{
				cmd.CommandText = query.QueryString;

				if (IsInTransaction && Transaction != null)
				{
					cmd.Transaction = Transaction;
				}

				cmd.Prepare();

				object obj = cmd.ExecuteScalar();

				if (obj is TDestination)
				{
					return (TDestination)obj;
				}
				else
				{
					TDestination _result = default(TDestination);
					try
					{
						_result = (TDestination)Convert.ChangeType(obj, typeof(TDestination));
					}
					catch (Exception ex)
					{
						throw new SqlDataMapperException(String.Format("Invalid cast. Type '{0}' required.", obj.GetType()), ex);
					}
					return _result;
				}
			}
		}

		/// <summary>
		/// Query for a list of objects.
		/// </summary>
		/// <typeparam name="TDestination">The destination type.</typeparam>
		/// <param name="query">The query.</param>
		/// <returns>List of destination objects.</returns>
		private IEnumerable<TDestination> QueryObjectList<TDestination>(ISqlQuery query) where TDestination : class, new()
		{
			using (IDbCommand cmd = Connection.CreateCommand())
			{
				cmd.CommandText = query.QueryString;

				if (IsInTransaction && Transaction != null)
				{
					cmd.Transaction = Transaction;
				}

				cmd.Prepare();

				var mapper = new SqlMapper<TDestination>();
				using (IDataReader reader = cmd.ExecuteReader())
				{
					var result = new List<TDestination>();
					while (reader.Read())
					{
						result.Add(mapper.MapFrom(reader));
					}
					return result;
				}
			}
		}

		/// <summary>
		/// Query for an object.
		/// </summary>
		/// <typeparam name="TDestination">The destination type.</typeparam>
		/// <param name="query">The query.</param>
		/// <returns>One destination object.</returns>
		private TDestination QueryObject<TDestination>(ISqlQuery query) where TDestination : class, new()
		{
			using (IDbCommand cmd = Connection.CreateCommand())
			{
				cmd.CommandText = query.QueryString;

				if (IsInTransaction && Transaction != null)
				{
					cmd.Transaction = Transaction;
				}

				cmd.Prepare();

				var mapper = new SqlMapper<TDestination>();
				using (IDataReader reader = cmd.ExecuteReader())
				{
					if (reader.Read())
					{
						return mapper.MapFrom(reader);
					}
					else
					{
						return default(TDestination);
					}
				}
			}
		}

		/// <summary>
		/// Executes a non query.
		/// </summary>
		/// <param name="query">The query.</param>
		/// <returns>Affected rows.</returns>
		private int ExecuteNonQuery(ISqlQuery query)
		{
			using (IDbCommand cmd = Connection.CreateCommand())
			{
				cmd.CommandText = query.QueryString;

				if (IsInTransaction && Transaction != null)
				{
					cmd.Transaction = Transaction;
				}

				cmd.Prepare();

				return cmd.ExecuteNonQuery();
			}
		}
	}
}
