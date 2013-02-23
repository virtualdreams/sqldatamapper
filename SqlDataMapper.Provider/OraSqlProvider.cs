using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Configuration;
using System.Data;

using Devart.Data;
using Devart.Data.Oracle;

namespace SqlDataMapper.Provider
{
	/// <summary>
	/// The provider for Oracle because default schema settings on connection startup.
	/// </summary>
    public class OraSqlProvider: ISqlProvider
    {
        private OracleConnection m_Connection = null;
        private OracleTransaction m_Transaction = null;
        private string m_SchemaName = "";
        
        /// <summary>
		/// Create a new sql connection class. The assembly would automatic loaded if possible.
        /// </summary>
		/// <param name="connectionString">The connection string to the database, i.e. <c>User Id=username;Password=password;Server=hostname;Direct=True;Service Name=servicename;Port=port;Pooling=true</c></param>
        public OraSqlProvider(string connectionString):
			this(connectionString, null)
        {
        }
        
        /// <summary>
		/// Create a new sql connection class. The assembly would automatic loaded if possible.
        /// </summary>
		/// <param name="connectionString">The connection string to the database, i.e. <c>User Id=username;Password=password;Server=hostname;Direct=True;Service Name=servicename;Port=port;Pooling=true</c></param>
        /// <param name="schemaName">The schemaname to use for this connection.</param>
        public OraSqlProvider(string connectionString, string schemaName)
        {
			try
			{
				m_SchemaName = schemaName;
				m_Connection = new OracleConnection(connectionString);
			}
			catch(Exception ex)
			{
				throw new Exception(String.Format("Can't create database object: {0}", ex.Message), ex);
			}
        }

		/// <summary>
		/// Open the database connection
		/// </summary>
        public void Open()
        {
            Close();
            try
            {
				m_Connection.Open();
				if(!String.IsNullOrEmpty(m_SchemaName))
					SetSchema(m_SchemaName);
            }
            catch (Exception ex)
            {
                Close();
                throw new Exception(String.Format("Can't open database: {0}", ex.Message), ex);
            }
        }
		
		/// <summary>
		/// Close the database connection
		/// </summary>
        public void Close()
        {
            if (m_Connection != null)
            {
                m_Connection.Close();
            }
        }

		/// <summary>
		/// Begins a database transaction
		/// </summary>
		public void BeginTransaction()
		{
			try
			{
				m_Transaction = m_Connection.BeginTransaction();
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
		/// Set the schema for the current database session
		/// </summary>
		/// <param name="schema">The schemaname to set.</param>
        private void SetSchema(string schema)
        {
            using (OracleCommand cmd = m_Connection.CreateCommand())
            {
                cmd.CommandText = String.Format("ALTER SESSION SET CURRENT_SCHEMA={0}", Escape(schema));
                cmd.ExecuteNonQuery();
            }
        }
		
		/// <summary>
		/// Escape parameters for direct input to database
		/// </summary>
        public string Escape(string str)
        {
            return str.Replace("\\", "\\\\").Replace("'", "\\'").Replace("\"", "\\\"");
        }
        
        /// <summary>
        /// Request a single row or the first result for the given statement
        /// </summary>
        public T Select<T>(string query)
        {
			try
			{
				using (OracleCommand cmd = m_Connection.CreateCommand())
				{
					cmd.CommandText = query;

					if (m_Transaction != null)
					{
						cmd.Transaction = m_Transaction;
					}
					
					cmd.Prepare();

					using(OracleDataReader reader = cmd.ExecuteReader())
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
			catch (Exception ex)
			{
				throw ex;
			}
        }
        
        /// <summary>
        /// Request a result set for the given sql statement
        /// </summary>
        public List<T> SelectList<T>(string query)
        {
			try
			{
				using (OracleCommand cmd = m_Connection.CreateCommand())
				{
					cmd.CommandText = query;

					if (m_Transaction != null)
					{
						cmd.Transaction = m_Transaction;
					}
					
					cmd.Prepare();
					
					using(OracleDataReader reader = cmd.ExecuteReader())
					{
						List<T> list = new List<T>();
						while(reader.Read())
						{
							list.Add(SqlObject.GetAs<T>(reader));
						}
						return list;
					}
				}
			}
			catch (Exception ex)
			{
				throw ex;
			}
        }

		/// <summary>
		/// Selects the first row and the first column in resultset.
		/// </summary>
		public T SelectScalar<T>(string query)
		{
			try
			{
				using (OracleCommand cmd = m_Connection.CreateCommand())
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
							throw new Exception(String.Format("Invalid cast. Type '{0}' is required.", obj.GetType()), ex);
							//return default(T);
						}
					}
				}
			}
			catch (Exception ex)
			{
				throw ex;
			}
		}
		
		/// <summary>
		/// Insert
		/// </summary>
		public int Insert(string query)
		{
			try
			{
				using (OracleCommand cmd = m_Connection.CreateCommand())
				{
					cmd.CommandText = query;

					if (m_Transaction != null)
					{
						cmd.Transaction = m_Transaction;
					}
					
					cmd.Prepare();

					return cmd.ExecuteNonQuery();
				}
			} catch (Exception ex)
			{
				throw ex;
			}
		}
		
		/// <summary>
		/// Update
		/// </summary>
		public int Update(string query)
		{
			try
			{
				using (OracleCommand cmd = m_Connection.CreateCommand())
				{
					cmd.CommandText = query;

					if (m_Transaction != null)
					{
						cmd.Transaction = m_Transaction;
					}
					
					cmd.Prepare();

					return cmd.ExecuteNonQuery();
				}
			} catch (Exception ex)
			{
				throw ex;
			}
		}
		
		/// <summary>
		/// Delete
		/// </summary>
		public int Delete(string query)
		{
			try
			{
				using (OracleCommand cmd = m_Connection.CreateCommand())
				{
					cmd.CommandText = query;

					if (m_Transaction != null)
					{
						cmd.Transaction = m_Transaction;
					}
					
					cmd.Prepare();

					return cmd.ExecuteNonQuery();
				}
			} catch (Exception ex)
			{
				throw ex;
			}
		}
	}
}
