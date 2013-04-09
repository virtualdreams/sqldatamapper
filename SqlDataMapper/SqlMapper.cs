using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;
using System.Xml.Linq;
using System.Text.RegularExpressions;
using System.Collections.Specialized;
using System.Data;
using System.Collections;
using System.Xml.Schema;
using System.IO;
using System.Reflection;
//using System.Web;
using System.Diagnostics;

namespace SqlDataMapper
{
	/// <summary>
	/// The core class for sql mapping. It contains all necessary methods for easy interacting with sql servers.
	/// </summary>
	public class SqlMapper
	{
		private ISqlProvider m_Provider = null;
		private HybridDictionary m_Statements = new HybridDictionary();
		private HybridDictionary m_Providers = new HybridDictionary();
		private bool m_IsTransactionSession = false;
		private bool m_ParameterCheck = false;
		private bool m_ValidationCheck = false;
		
		#region Structures
		private struct Statement
		{
			public string statement;
			public string classname;
		};

		private struct Provider
		{
			public string assemblyName;
			public string connectionClass;
		};
		#endregion
		
		#region Validation methods
		
		/// <summary>
		/// Enable or disable parameter check after replace
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
		
		/// <summary>
		/// Enable or disable xml validation.
		/// Mono 2.6.x has a bug on GetResourceFromStream().
		/// This feature is completely disabled at the moment.
		/// </summary>
		public bool ValidationCheck
		{
			get
			{
				return m_ValidationCheck;
			}
			set
			{
				m_ValidationCheck = value;
			}
		}
		
		#endregion
		
		#region Constructors
		
		/// <summary>
		/// Constructor for auto configuration (zeroconf).
		/// Searchs for <c>SqlMapperConfig.xml</c> file, otherwise throws an excpetion.
		/// </summary>
		public SqlMapper()
		{
			string file = ConvertToFullPath("SqlMapperConfig.xml");
			
			FileInfo fi = new FileInfo(file);
			if(fi.Exists)
			{
				LoadConfiguration(fi.FullName);
			}
			else
			{
				throw new SqlDataMapperException("Can't find 'SqlMapperConfig.xml' for autoconfiguration.");
			}
		}
		
		/// <summary>
		/// Constructor for auto configuration.
		/// </summary>
		/// <param name="configXml">The configuration file</param>
		public SqlMapper(string configXml)
		{
			if(String.IsNullOrEmpty(configXml))
				throw new ArgumentNullException("configXml");
			
			LoadConfiguration(configXml);
		}
		
		/// <summary>
		/// Constructor for non auto configurarion. Instantiate your own provider. This instance has no statements and must filled manually or with embeded statements.
		/// </summary>
		/// <param name="provider">The provider <ref>SqlProvider</ref> or your own <ref>ISqlProvider</ref></param>
		public SqlMapper(ISqlProvider provider)
		{
			if(provider == null)
				throw new ArgumentNullException("provider");
				
			m_Provider = provider;
		}

		/// <summary>
		/// Constructor for non auto configuration. Instantiate your own provider an load your own mappings.
		/// </summary>
		/// <param name="provider">The provider <ref>SqlProvider</ref> or your own <ref>ISqlProvider</ref></param>
		/// <param name="mappingXml">The xml file containing the xml statements</param>
		public SqlMapper(ISqlProvider provider, string mappingXml)
		{
			if(provider == null)
				throw new ArgumentNullException("provider");
				
			if(String.IsNullOrEmpty(mappingXml))
				throw new ArgumentNullException("mappingXml");

			m_Provider = provider;
			LoadMappings(mappingXml);
		}
		
		#endregion

		#region Configuration methods
		
		/// <summary>
		/// Load the configuration xml file.
		/// </summary>
		/// <param name="filename">The xml file contains the base configuration</param>
		private void LoadConfiguration(string filename)
		{
			try
			{
				filename = ConvertToFullPath(filename);

				XDocument doc = XDocument.Load(filename, LoadOptions.None);
				
				if(ValidationCheck)
					ValidateXml(doc, "SqlDataMapper.configuration.xsd");

				ReadConfiguration(doc);
			}
			catch (Exception ex)
			{
				throw new SqlDataMapperException(String.Format("Load configuration file failed: '{0}' -> {1}", filename, ex.Message), ex);
			}
		}
		
		/// <summary>
		/// Load the providers xml file.
		/// </summary>
		/// <param name="filename">The xml file contains the provider informations</param>
		private void LoadProviders(string filename)
		{
			try
			{
				filename = ConvertToFullPath(filename);
				
				XDocument doc = XDocument.Load(filename, LoadOptions.None);

				if (ValidationCheck)
					ValidateXml(doc, "SqlDataMapper.provider.xsd");
				
				ReadProviders(doc);
			}
			catch(Exception ex)
			{
				throw new SqlDataMapperException(String.Format("Load providers file failed: '{0}' -> {1}", filename, ex.Message), ex);
			}
		}

		/// <summary>
		/// Load the mappings xml file.
		/// </summary>
		/// <param name="filename">The xml file contains the statements</param>
		public void LoadMappings(string filename)
		{
			try
			{
				filename = ConvertToFullPath(filename);

				XDocument doc = XDocument.Load(filename, LoadOptions.None);

				if (ValidationCheck)
					ValidateXml(doc, "SqlDataMapper.mapping.xsd");

				LoadSelects(doc);
				LoadInserts(doc);
				LoadUpdates(doc);
				LoadDeletes(doc);
				LoadSegments(doc);
				LoadInclude(doc);
			}
			catch (Exception ex)
			{
				throw new SqlDataMapperException(String.Format("Load sql map failed: '{0}' -> {1}", filename, ex.Message), ex);
			}
		}
		
		/// <summary>
		/// Read configuration file for auto config.
		/// </summary>
		/// <param name="doc">A xml document contains the base configuration</param>
		private void ReadConfiguration(XDocument doc)
		{
			string providers = doc.Element("sqlMapConfig").Element("providers").Attribute("file").Value;
			var database = doc.Element("sqlMapConfig").Element("database");
			string providerName = database.Attribute("provider").Value;
			string connectionString = database.Attribute("connectionString").Value;
			
			//Load providers
			LoadProviders(providers);
			
			//Load mappings
			var maps = from map in doc.Element("sqlMapConfig").Element("sqlMaps").Elements("sqlMap")
					   select new
					   {
							file = map.Attribute("file").Value
					   };

			foreach (var map in maps)
			{
			    string file = map.file.Trim();

			    LoadMappings(file);
			}
			
			Provider provider = GetProvider(providerName);
			
			m_Provider = new SqlProvider(provider.assemblyName, provider.connectionClass, connectionString);
		}
		
		/// <summary>
		/// Read provider informations.
		/// </summary>
		/// <param name="doc">A xml document contains the provider informations</param>
		private void ReadProviders(XDocument doc)
		{
			var providers = from provider in doc.Element("providers").Elements("provider")
							select new
							{
								id = provider.Attribute("id").Value,
								assemblyName = provider.Attribute("assemblyName").Value,
								connectionClass = provider.Attribute("connectionClass").Value
							};
			
			foreach(var provider in providers)
			{
				string id = provider.id.Trim();
				string assemblyName = provider.assemblyName.Trim();
				string connectionClass = provider.connectionClass.Trim();
				
				AddProvider(id, new Provider { assemblyName = assemblyName, connectionClass = connectionClass });
			}
		}
		
		/// <summary>
		/// Validate the xml document.
		/// Throws XmlSchemaException if fails
		/// </summary>
		/// <param name="doc">The xml document</param>
		/// <param name="xsd">The xsd stream name</param>
		private void ValidateXml(XDocument doc, string xsd)
		{
			if (String.IsNullOrEmpty(xsd))
				throw new ArgumentNullException("xsd");

			XmlSchemaSet schema = new XmlSchemaSet();
			Stream xsdstream = Assembly.GetExecutingAssembly().GetManifestResourceStream(xsd);
			if (xsdstream != null)
			{
				schema.Add(null, XmlReader.Create(xsdstream));
			}
			
			doc.Validate(schema, (o, e) =>
			{
				throw new XmlSchemaException(e.Message);
			});
		}
		
		/// <summary>
		/// Get the provider information for a given id.
		/// </summary>
		/// <param name="id">The name of the provider.</param>
		/// <returns>Returns the provider.</returns>
		private Provider GetProvider(string id)
		{
			if(String.IsNullOrEmpty(id))
				throw new ArgumentNullException("id");
			
			if(!m_Providers.Contains(id))
			{
				throw new SqlDataMapperException(String.Format("This provider map does not contain a provider named '{0}'", id));
			}
			Provider provider = (Provider)m_Providers[id];
			return provider;
		}

		/// <summary>
		/// Get the full path for the given filename.
		/// </summary>
		/// <param name="filename">The filename as file, absolute filepath or relative filepath</param>
		/// <returns>The full path</returns>
		private string ConvertToFullPath(string filename)
		{
			if (String.IsNullOrEmpty(filename))
			{
				throw new ArgumentNullException("filename");
			}

			string tmp = "";
			try
			{
				string applicationBaseDirectory = AppDomain.CurrentDomain.BaseDirectory;
				if (applicationBaseDirectory != null)
				{
					Uri uri = new Uri(applicationBaseDirectory);
					if (uri.IsFile)
					{
						tmp = uri.LocalPath;
					}
				}
			}
			catch
			{
			}
			if (!String.IsNullOrEmpty(tmp))
			{
				return Path.GetFullPath(Path.Combine(tmp, filename));
			}
			return Path.GetFullPath(filename);
		}
		#endregion
		
		#region Statement methods

		/// <summary>
		/// Get the named raw sql statement from the cache.
		/// </summary>
		/// <param name="id">The name of the sql statement</param>
		/// <returns>Returns the raw statement.</returns>
		public string GetStatement(string id)
		{
			if(String.IsNullOrEmpty(id))
				throw new ArgumentException("The parameter must contain an id.", "id");
			
			//get statement from pool
			if (!m_Statements.Contains(id))
			{
				throw new SqlDataMapperException(String.Format("This sql map does not contain a statement named '{0}'", id));
			}
			Statement st = (Statement)m_Statements[id];
			return st.statement;
		}
		
		/// <summary>
		/// Create a new dynamic query out of the statement pool
		/// </summary>
		/// <param name="id">The named sql</param>
		/// <returns>New SqlQuery object</returns>
		public SqlQuery CreateQuery(string id)
		{
			return new SqlQuery(GetStatement(id));
		}
		
		/// <summary>
		/// Create a new dynamic query out of the statement pool. This is for a custom query class
		/// </summary>
		/// <typeparam name="T">The object type of ISqlQuery</typeparam>
		/// <param name="id">The named sql</param>
		/// <returns>New ISqlQuery object</returns>
		public T CreateQuery<T>(string id) where T: ISqlQuery, new()
		{
			T t = new T();
			t.Set(GetStatement(id));
			return t;
		}
		
		#endregion
		
		#region Sql methods
		
		/// <summary>
		/// Begins a database transaction.
		/// </summary>
		/// <remarks>
		/// The method opens a connection to the datebase. The connection will closed through <c>CommitTransaction</c> or <c>RollbackTransaction</c>.
		/// </remarks>
		public void BeginTransaction()
		{
			if(m_IsTransactionSession)
			{
				throw new SqlDataMapperException("SqlMapper could not invoke BeginTransaction(). A transaction is already started. Call CommitTransaction() or RollbackTransaction() first.");
			}
			
			try
			{
				m_Provider.Open();
				m_IsTransactionSession = true;
				m_Provider.BeginTransaction();
			}
			catch(Exception ex)
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
			if(!m_IsTransactionSession)
			{
				throw new SqlDataMapperException("SqlMapper could not invoke CommitTransaction(). No transaction was started. Call BeginTransaction() first.");
			}
			
			try
			{
				m_Provider.CommitTransaction();
			}
			catch(Exception ex)
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
			if(!m_IsTransactionSession)
			{
				throw new SqlDataMapperException("SqlMapper could not invoke CommitTransaction(). No transaction was started. Call BeginTransaction() first.");
			}
			
			try
			{
				m_Provider.RollbackTransaction();
			}
			catch(Exception ex)
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
				if(!m_IsTransactionSession)
				{
					m_Provider.Open();
					flag = true;
				}

				//TODO: Statement class?
				return m_Provider.Select<T>(query.Check(this.ParameterCheck).QueryString);
			}
			catch(Exception ex)
			{
				throw ex;
			}
			finally
			{
				if(flag)
				{
					m_Provider.Close();
				}
			}
		}

		/// <summary>
		/// Executes a sql select statement that returns data to populate a number of result objects.
		/// </summary>
		/// <typeparam name="T">The object type</typeparam>
		/// <param name="query">The query object</param>
		/// <returns>A list ob objects</returns>
		public List<T> QueryForList<T>(ISqlQuery query)
		{
			bool flag = false;
			try
			{
				if (!m_IsTransactionSession)
				{
					m_Provider.Open();
					flag = true;
				}

				//TODO: Statement class?
				return m_Provider.SelectList<T>(query.Check(this.ParameterCheck).QueryString);
			}
			catch(Exception ex)
			{
				throw ex;
			}
			finally
			{
				if(flag)
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
				
				//TODO: Statement class?
				return m_Provider.SelectScalar<T>(query.Check(this.ParameterCheck).QueryString);
			}
			catch (Exception ex)
			{
				throw ex;
			}
			finally
			{
				if(flag)
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

				//TODO: Statement class?
				return m_Provider.Insert(query.Check(this.ParameterCheck).QueryString);
			}
			catch (Exception ex)
			{
				throw ex;
			}
			finally
			{
				if(flag)
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

				//TODO: Statement class?
				return m_Provider.Update(query.Check(this.ParameterCheck).QueryString);
			}
			catch (Exception ex)
			{
				throw ex;
			}
			finally
			{
				if(flag)
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

				//TODO: Statement class?
				return m_Provider.Delete(query.Check(this.ParameterCheck).QueryString);
			}
			catch (Exception ex)
			{
				throw ex;
			}
			finally
			{
				if(flag)
				{
					m_Provider.Close();
				}
			}
		}

		#endregion
		
		#region Internal methods
		
		/// <summary>
		/// The class type must match with provided class name from the sql map
		/// </summary>
		private void CheckClassType()
		{
		//    if (!String.Equals(typeof(T).FullName, GetClassname(id)))
		//    {
		//        throw new Exception(String.Format("The class type '{0}' doesn't match with statement provided class type '{1}'", typeof(T).GetType().FullName, GetClassname(id)));
		//    }
		}
	
		/// <summary>
		/// Get the associated classname for the provided statement.
		/// </summary>
		/// <param name="id">The id that identifies the statement</param>
		/// <returns>Return the full qualified classname for statement</returns>
		private string GetClassname(string id)
		{
			if (!m_Statements.Contains(id))
			{
				throw new Exception(String.Format("This sql map does not contain a statement named '{0}'", id));
			}
			Statement st = (Statement)m_Statements[id];
			return st.classname;
		}

		/// <summary>
		/// Add a user defined statement to the statement pool.
		/// </summary>
		/// <param name="id">The unique identifier for the statement</param>
		/// <param name="statement">The object contains the statement informations</param>
		public void AddStatement(string id, string statement)
		{
			this.AddStatement(id, new Statement{ statement = statement, classname = "" });
		}

		/// <summary>
		/// Add a user defined statement to the statement pool.
		/// </summary>
		/// <param name="id">The unique identifier for the statement</param>
		/// <param name="statement">A string contains the statement</param>
		/// <param name="classname">A string contains the classname</param>
		public void AddStatement(string id, string statement, string classname)
		{
			this.AddStatement(id, new Statement { statement = statement, classname = classname });
		}
		
		/// <summary>
		/// Add a loaded statement to the statement pool.
		/// </summary>
		/// <param name="id">The unique identifier for the statement</param>
		/// <param name="statement">The object contains the statement informations</param>
		private void AddStatement(string id, Statement statement)
		{
			if(String.IsNullOrEmpty(id))
				throw new ArgumentException("The id can't be null or an empty string.", "id");
			
			if(m_Statements.Contains(id))
			{
				throw new SqlDataMapperException(String.Format("The sql map already contains a statement named '{0}'", id));
			}
			this.m_Statements.Add(id, statement);
		}
		
		/// <summary>
		/// Add a loaded provider to the provider pool.
		/// </summary>
		/// <param name="id">The unique identifier for the statement</param>
		/// <param name="provider">The object contains the provider informations</param>
		private void AddProvider(string id, Provider provider)
		{
			if (m_Providers.Contains(id))
			{
				throw new SqlDataMapperException(String.Format("The provider pool already contains a provider named '{0}'", id));
			}
			this.m_Providers.Add(id, provider);
		}
		
		/// <summary>
		/// Load all select statements out of the given xml file.
		/// </summary>
		/// <param name="doc">The document that contains the statements</param>
		private void LoadSelects(XDocument doc)
		{
			//get all select statements
			var selects = from query in doc.Element("sqlMap").Elements("select")
							select new 
							{
								id = query.Attribute("id").Value,
								cl = query.Attribute("class").Value,
								value = query.Value
							};
						  
			foreach(var select in selects)
			{
				string id = select.id.Trim();
				string value = select.value.Trim();
				string cl = select.cl.Trim();
				
				AddStatement(id, new Statement { statement = value, classname = cl });
			}
		}
		
		/// <summary>
		/// Load all insert statements out of the given xml file.
		/// </summary>
		/// <param name="doc">The document that contains the statements</param>
		private void LoadInserts(XDocument doc)
		{
			//get all insert statements		
			var inserts = from query in doc.Element("sqlMap").Elements("insert")
			                select new 
			                {
			                    id = query.Attribute("id").Value,
								cl = query.Attribute("class").Value,
			                    value = query.Value
			                };
						  
			foreach(var select in inserts)
			{
				string id = select.id.Trim();
				string value = select.value.Trim();
				string cl = select.cl.Trim();

				AddStatement(id, new Statement { statement = value, classname = cl });
			}
		}
		
		/// <summary>
		/// Load all update statements out of the given xml file.
		/// </summary>
		/// <param name="doc">The document that contains the statements</param>
		private void LoadUpdates(XDocument doc)
		{
			//get all update statements				
			var updates = from query in doc.Element("sqlMap").Elements("update")
			                select new 
			                {
			                    id = query.Attribute("id").Value,
								cl = query.Attribute("class").Value,
			                    value = query.Value
			                };
						  
			foreach(var select in updates)
			{
				string id = select.id.Trim();
				string value = select.value.Trim();
				string cl = select.cl.Trim();

				AddStatement(id, new Statement { statement = value, classname = cl });
			}
		}
		
		/// <summary>
		/// Load all delete statements out of the given xml file.
		/// </summary>
		/// <param name="doc">The document that contains the statements</param>
		private void LoadDeletes(XDocument doc)
		{
			//get all delete statements
			var deletes = from query in doc.Element("sqlMap").Elements("delete")
			                select new 
			                {
			                    id = query.Attribute("id").Value,
								cl = query.Attribute("class").Value,
			                    value = query.Value
			                };
						  
			foreach(var select in deletes)
			{
				string id = select.id.Trim();
				string value = select.value.Trim();
				string cl = select.cl.Trim();

				AddStatement(id, new Statement { statement = value, classname = cl });
			}
		}

		/// <summary>
		/// Load all fragment statements out of the given xml file.
		/// </summary>
		/// <param name="doc">The document that contains the statements</param>
		private void LoadSegments(XDocument doc)
		{
			//get all delete statements
			var deletes = from query in doc.Element("sqlMap").Elements("segment")
						  select new
						  {
							  id = query.Attribute("id").Value,
							  cl = query.Attribute("class").Value,
							  value = query.Value
						  };

			foreach (var select in deletes)
			{
				string id = select.id.Trim();
				string value = select.value.Trim();
				string cl = select.cl.Trim();

				AddStatement(id, new Statement { statement = value, classname = cl });
			}
		}
		
		/// <summary>
		/// Load included documents.
		/// </summary>
		/// <param name="doc">The document that contains the statements</param>
		private void LoadInclude(XDocument doc)
		{
			var include = from query in doc.Element("sqlMap").Elements("include")
							select new
							{
								file = query.Attribute("file").Value
							};
							
			foreach(var select in include)
			{
				string file = select.file.Trim();
				LoadMappings(file);
			}
		}
		#endregion
	}
}
