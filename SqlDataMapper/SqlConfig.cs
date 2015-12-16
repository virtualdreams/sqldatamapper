using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Reflection;
using System.Diagnostics;

namespace SqlDataMapper
{
	/// <summary>
	/// The config class for sql mapping.
	/// </summary>
	public class SqlConfig
	{
		private HybridDictionary _Statements = new HybridDictionary();
		private HybridDictionary _Providers = new HybridDictionary();

		/// <summary>
		/// Get or set default provider id.
		/// </summary>
		public string DefaultProvider { get; set; }

		/// <summary>
		/// Get or set default connection string.
		/// </summary>
		public string DefaultConnectionString { get; set; }

		/// <summary>
		/// Product version
		/// </summary>
		public static string Version
		{
			get
			{
				Assembly assembly = Assembly.GetExecutingAssembly();
				FileVersionInfo fvi = FileVersionInfo.GetVersionInfo(assembly.Location);

				return fvi.ProductVersion;
			}
		}
		
		private struct Statement
		{
			public string statement;
		};

		private struct Provider
		{
			public string assemblyName;
			public string connectionClass;
		};
		
		/// <summary>
		/// Create an empty instance.
		/// </summary>
		public SqlConfig()
		{ }
		
		/// <summary>
		/// Create an instance and load configuration from file.
		/// </summary>
		/// <param name="filename">The configuration file.</param>
		public SqlConfig(string filename)
		{
			new SqlXmlLoader(this).Configure(filename);
		}

		/// <summary>
		/// Add a provider to configuration.
		/// <example>assemblyName: MySql.Data</example>
		/// <example>connectionClass: MySql.Data.MySqlClient.MySqlConnection</example>
		/// </summary>
		/// <param name="id">The unique identifier for the provider.</param>
		/// <param name="assemblyName">The assembly name.</param>
		/// <param name="connectionClass">The connection class.</param>
		public void AddProvider(string id, string assemblyName, string connectionClass)
		{
			if(String.IsNullOrEmpty(assemblyName))
				throw new ArgumentNullException("assemblyName");

			if(String.IsNullOrEmpty(connectionClass))
				throw new ArgumentNullException("connectionClass");

			this.AddProvider(id, new Provider { assemblyName = assemblyName, connectionClass = connectionClass });
		}
		
		/// <summary>
		/// AAdd a provider to configuration.
		/// </summary>
		/// <param name="id">The unique identifier for the provider.</param>
		/// <param name="provider">The provider data.</param>
		private void AddProvider(string id, Provider provider)
		{
			if (String.IsNullOrEmpty(id))
				throw new ArgumentNullException("id");

			if (_Providers.Contains(id))
			{
				throw new SqlDataMapperException(String.Format("The configuration already contains a provider named '{0}'.", id));
			}
			this._Providers.Add(id, provider);
		}

		/// <summary>
		/// Get provider data for id.
		/// </summary>
		/// <param name="id">The name of the provider.</param>
		/// <returns></returns>
		private Provider GetProvider(string id)
		{
			if (String.IsNullOrEmpty(id))
				throw new ArgumentNullException("id");

			if (!_Providers.Contains(id))
			{
				throw new SqlDataMapperException(String.Format("This configuration does not contain a provider named '{0}'.", id));
			}
			return (Provider)_Providers[id];
		}

		/// <summary>
		/// Get all registred provider id's.
		/// </summary>
		/// <returns></returns>
		public IEnumerable<string> GetProviders()
		{
			foreach (string key in _Providers.Keys)
			{
				yield return key;
			}
		}

		/// <summary>
		/// Remove provider from configuration.
		/// </summary>
		/// <param name="id">The provider id.</param>
		public void RemoveProvider(string id)
		{
			if (String.IsNullOrEmpty(id))
				throw new ArgumentNullException("id");

			if (_Providers.Contains(id))
			{
				_Providers.Remove(id);
			}
		}

		/// <summary>
		/// Remove all providers from configuration.
		/// </summary>
		public void ClearProviders()
		{
			_Providers.Clear();
		}

		/// <summary>
		/// Add a statement to configuration.
		/// </summary>
		/// <param name="id">The unique identifier for the statement.</param>
		/// <param name="statement">The statement.</param>
		public void AddStatement(string id, string statement)
		{
			if(String.IsNullOrEmpty(statement))
				throw new ArgumentNullException("statement");
			
			this.AddStatement(id, new Statement{ statement = statement });
		}
		
		/// <summary>
		/// Add a statement to configuration.
		/// </summary>
		/// <param name="id">The unique identifier for the statement</param>
		/// <param name="statement">The statement.</param>
		private void AddStatement(string id, Statement statement)
		{
			if(String.IsNullOrEmpty(id))
				throw new ArgumentNullException("id");
			
			if(_Statements.Contains(id))
			{
				throw new SqlDataMapperException(String.Format("The configuration already contains a statement named '{0}'.", id));
			}
			this._Statements.Add(id, statement);
		}

		/// <summary>
		/// Get the statement from configuration.
		/// </summary>
		/// <param name="id">The statement id.</param>
		/// <returns></returns>
		public string GetStatement(string id)
		{
			if(String.IsNullOrEmpty(id))
				throw new ArgumentNullException("id");
			
			//get statement from pool
			if (!_Statements.Contains(id))
			{
				throw new SqlDataMapperException(String.Format("This configuration does not contain a statement named '{0}'", id));
			}
			Statement st = (Statement)_Statements[id];
			return st.statement;
		}

		/// <summary>
		/// Get all registered statement id's.
		/// </summary>
		/// <returns></returns>
		public IEnumerable<string> GetStatments()
		{
			foreach (string key in _Statements.Keys)
			{
				yield return key;
			}
		}

		/// <summary>
		/// Remove a statement from configuration.
		/// </summary>
		/// <param name="id">The statement id.</param>
		public void RemoveStatement(string id)
		{
			if (String.IsNullOrEmpty(id))
				throw new ArgumentNullException("id");

			if (_Statements.Contains(id))
			{
				_Statements.Remove(id);
			}
		}

		/// <summary>
		/// Remove all statements from configuration.
		/// </summary>
		public void ClearStatements()
		{
			_Statements.Clear();
		}
		
		/// <summary>
		/// Create a new dynamic query out of the statement pool
		/// </summary>
		/// <param name="id">The named sql.</param>
		/// <returns></returns>
		public SqlQuery CreateQuery(string id)
		{
			return new SqlQuery(GetStatement(id));
		}
		
		/// <summary>
		/// Create a new dynamic query out of the statement pool. This is for a custom query class
		/// </summary>
		/// <typeparam name="T">The object type of ISqlQuery.</typeparam>
		/// <param name="id">The named sql.</param>
		/// <returns></returns>
		public T CreateQuery<T>(string id) where T: ISqlQuery, new()
		{
			T t = new T();
			t.Set(GetStatement(id));
			return t;
		}

		/// <summary>
		/// Create a new context using the default provider from configuration.
		/// </summary>
		public SqlContext CreateContext()
		{
			if (String.IsNullOrEmpty(DefaultProvider))
				throw new SqlDataMapperException("No default provider configured.");

			if (String.IsNullOrEmpty(DefaultConnectionString))
				throw new SqlDataMapperException("No default connection string configured.");

			return CreateContext(DefaultProvider, DefaultConnectionString);
		}
		
		/// <summary>
		/// Create a new context using custom provider and connection string.
		/// </summary>
		/// <param name="id">A provider from configuration</param>
		/// <param name="connectionString">Custom connection string</param>
		/// <returns></returns>
		public SqlContext CreateContext(string id, string connectionString)
		{
			if (String.IsNullOrEmpty(id))
				throw new ArgumentNullException("id");

			if (String.IsNullOrEmpty(connectionString))
				throw new ArgumentNullException("connectionString");

			Provider provider = GetProvider(id);

			return new SqlContext(provider.assemblyName, provider.connectionClass, connectionString);
		}
	}
}
