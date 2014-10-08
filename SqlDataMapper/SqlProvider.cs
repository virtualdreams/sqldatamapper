using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Configuration;
using System.Data;
using System.Data.Common;

namespace SqlDataMapper
{
    /// <summary>
    /// The default sql provider.
    /// </summary>
    public class SqlProvider: ISqlProvider
    {
        private DbConnection m_Connnection = null;
        private DbTransaction m_Transaction = null;
        
        /// <summary>
        /// Create a new sql connection class. The assembly would automatic loaded if possible.
        /// </summary>
		/// <param name="assemblyName">The assembly name, i.e. <c>MySql.Data</c></param>
		/// <param name="connectionClass">The connection class, i.e. <c>MySql.Data.MySqlClient.MySqlConnection</c></param>
		/// <param name="connectionString">The connection string to the database, i.e. <c>Server=hostname;Database=dbname;Uid=user;Pwd=password;Pooling=true</c></param>
        public SqlProvider(string assemblyName, string connectionClass, string connectionString)
        {
			try
			{
				m_Connnection = (DbConnection)Activator.CreateInstance(assemblyName, connectionClass).Unwrap();
				m_Connnection.ConnectionString = connectionString;
			}
			catch(Exception ex)
			{
				throw new Exception(String.Format("Can't create database object: {0}", ex.Message), ex);
			}
        }
		
		/// <summary>
		/// Opens a connection to the database
		/// </summary>
		/// <remarks>
		/// If a connection already exits, the connection was closed before.
		/// </remarks>
        public void Open()
        {
            Close();
            try
            {
				m_Connnection.Open();
            }
            catch (Exception ex)
            {
                Close();
				throw new Exception(String.Format("Can't open datenbase: {0}", ex.Message));
            }
        }

		/// <summary>
		/// Close the connection to the database
		/// </summary>
        public void Close()
        {
            if (m_Connnection != null)
            {
                m_Connnection.Close();
            }
        }
		
		/// <summary>
		/// Begins a database transaction
		/// </summary>
		public void BeginTransaction()
		{
			try
			{
				m_Transaction = m_Connnection.BeginTransaction();
			}
			catch (Exception ex)
			{
				m_Transaction = null;
				throw ex;
			}
		}
		
		/// <summary>
		/// Commits the database transaction.
		/// </summary>
		public void CommitTransaction()
		{
			if (m_Transaction != null)
			{
				m_Transaction.Commit();
				m_Transaction.Dispose();
				m_Transaction = null;
			}
		}
		
		/// <summary>
		/// Rolls back a transaction from a pending state.
		/// </summary>
		public void RollbackTransaction()
		{
			if (m_Transaction != null)
			{
				m_Transaction.Rollback();
				m_Transaction.Dispose();
				m_Transaction = null;
			}
		}
		
        /// <summary>
        /// Select a single object from the database.
        /// </summary>
		public T Select<T>(string query) where T : class, new()
		{
			using (DbCommand cmd = m_Connnection.CreateCommand())
			{
				cmd.CommandText = query;

				if (m_Transaction != null)
				{
					cmd.Transaction = m_Transaction;
				}

				cmd.Prepare();

				using (DbDataReader reader = cmd.ExecuteReader())
				{
					if (reader.HasRows)
					{
						reader.Read();

						return SqlObject.GetAs<T>(reader);
					}
					else
					{
						return default(T);
					}
				}
			}
		}
        
        /// <summary>
        /// Selects a list of objects from the database.
        /// </summary>
		public IEnumerable<T> SelectList<T>(string query) where T : class, new()
		{
			using (DbCommand cmd = m_Connnection.CreateCommand())
			{
				cmd.CommandText = query;

				if (m_Transaction != null)
				{
					cmd.Transaction = m_Transaction;
				}

				cmd.Prepare();

				using (DbDataReader reader = cmd.ExecuteReader())
				{
					while (reader.Read())
					{
						yield return SqlObject.GetAs<T>(reader);
					}
				}
			}
		}
        
        /// <summary>
        /// Selects the first row and the first column in resultset.
        /// </summary>
		public T SelectScalar<T>(string query) where T : IConvertible
		{
			using (DbCommand cmd = m_Connnection.CreateCommand())
			{
				cmd.CommandText = query;

				if (m_Transaction != null)
				{
					cmd.Transaction = m_Transaction;
				}

				cmd.Prepare();

				object obj = cmd.ExecuteScalar();

				if (obj is T)
				{
					return (T)obj;
				}
				else
				{
					try
					{
						return (T)Convert.ChangeType(obj, typeof(T));
					}
					catch (InvalidCastException ex)
					{
						throw new Exception(String.Format("Invalid cast. Type '{0}' required.", obj.GetType()), ex);
					}
				}
			}
		}

		/// <summary>
		/// Selects the first column and each row.
		/// </summary>
		public IEnumerable<T> SelectScalarList<T>(string query) where T : IConvertible
		{
			using (DbCommand cmd = m_Connnection.CreateCommand())
			{
				cmd.CommandText = query;

				if (m_Transaction != null)
				{
					cmd.Transaction = m_Transaction;
				}

				cmd.Prepare();

				using (DbDataReader reader = cmd.ExecuteReader())
				{
					List<T> list = new List<T>();
					while (reader.Read())
					{

						object obj = reader.GetValue(0);

						if (obj is T)
						{
							list.Add((T)obj);
						}
						else
						{
							try
							{
								list.Add((T)Convert.ChangeType(obj, typeof(T)));
							}
							catch (InvalidCastException ex)
							{
								throw new Exception(String.Format("Invalid cast. Type '{0}' is required.", obj.GetType()), ex);
							}
						}
					}
					return list.ToArray();
				}
			}
		}
		
		/// <summary>
		/// Insert
		/// </summary>
		public int Insert(string query)
		{
			using (DbCommand cmd = m_Connnection.CreateCommand())
			{
				cmd.CommandText = query;

				if (m_Transaction != null)
				{
					cmd.Transaction = m_Transaction;
				}

				cmd.Prepare();

				return cmd.ExecuteNonQuery();
			}
		}
		
		/// <summary>
		/// Update
		/// </summary>
		public int Update(string query)
		{
			using (DbCommand cmd = m_Connnection.CreateCommand())
			{
				cmd.CommandText = query;

				if (m_Transaction != null)
				{
					cmd.Transaction = m_Transaction;
				}

				cmd.Prepare();

				return cmd.ExecuteNonQuery();
			}
		}
		
		/// <summary>
		/// Delete
		/// </summary>
		public int Delete(string query)
		{
			using (DbCommand cmd = m_Connnection.CreateCommand())
			{
				cmd.CommandText = query;

				if (m_Transaction != null)
				{
					cmd.Transaction = m_Transaction;
				}

				cmd.Prepare();

				return cmd.ExecuteNonQuery();
			}
		}
	}
}
