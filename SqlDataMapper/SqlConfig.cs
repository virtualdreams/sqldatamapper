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
using System.Diagnostics;

namespace SqlDataMapper
{
	/// <summary>
	/// The config class for sql mapping.
	/// </summary>
	public class SqlConfig
	{
		private HybridDictionary m_Statements = new HybridDictionary();
		private HybridDictionary m_Providers = new HybridDictionary();
		private bool m_ValidationCheck = false;
		private string _defaultProvider = null;
		private string _defaultConnectionString = null;
		
		#region Structures
		private struct Statement
		{
			public string statement;
		};

		private struct Provider
		{
			public string assemblyName;
			public string connectionClass;
		};
		#endregion
		
		#region Validation methods
		
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
		/// Searchs for <c>SqlMapperConfig.xml</c> file, otherwise throws an exception.
		/// </summary>
		public SqlConfig()
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
		/// <param name="config">The configuration file</param>
		public SqlConfig(string config)
		{
			if(String.IsNullOrEmpty(config))
				throw new ArgumentNullException("config");
			
			LoadConfiguration(config);
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
		public void LoadQueries(string filename)
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
			
			//Load sql queries
			var maps = from map in doc.Element("sqlMapConfig").Element("sqlMaps").Elements("sqlMap")
					   select new
					   {
							file = map.Attribute("file").Value
					   };

			foreach (var map in maps)
			{
			    string file = map.file.Trim();

			    LoadQueries(file);
			}
			
			_defaultProvider = providerName;
			_defaultConnectionString = connectionString;
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
				throw new ArgumentNullException("id");
			
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

		/// <summary>
		/// Create a new context using the default provider from configuration.
		/// </summary>
		public SqlContext CreateContext()
		{
			if (String.IsNullOrEmpty(_defaultProvider))
				throw new SqlDataMapperException("No default provider configured.");

			if (String.IsNullOrEmpty(_defaultConnectionString))
				throw new SqlDataMapperException("No default connection string configured.");

			return CreateContext(_defaultProvider, _defaultConnectionString);
		}
		
		/// <summary>
		/// Create a new context using custom provider and connection string.
		/// </summary>
		/// <param name="id">A provider from configuration</param>
		/// <param name="connectionString">Custom connection string.</param>
		public SqlContext CreateContext(string id, string connectionString)
		{
			if (String.IsNullOrEmpty(id))
				throw new ArgumentNullException("id");

			if (String.IsNullOrEmpty(connectionString))
				throw new ArgumentNullException("connectionString");

			Provider provider = GetProvider(id);
			return new SqlContext(provider.assemblyName, provider.connectionClass, connectionString);
		}
		
		#endregion
		
		#region Internal methods

		/// <summary>
		/// Add a user defined statement to the statement pool.
		/// </summary>
		/// <param name="id">The unique identifier for the statement</param>
		/// <param name="statement">The object contains the statement informations</param>
		public void AddStatement(string id, string statement)
		{
			this.AddStatement(id, new Statement{ statement = statement /*, classname = ""*/ });
		}
		
		/// <summary>
		/// Add a loaded statement to the statement pool.
		/// </summary>
		/// <param name="id">The unique identifier for the statement</param>
		/// <param name="statement">The object contains the statement informations</param>
		private void AddStatement(string id, Statement statement)
		{
			if(String.IsNullOrEmpty(id))
				throw new ArgumentNullException("id");
			
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
            if (String.IsNullOrEmpty(id))
                throw new ArgumentNullException("id");
            
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
								value = query.Value
							};
						  
			foreach(var select in selects)
			{
				string id = select.id.Trim();
				string value = select.value.Trim();
				
				AddStatement(id, new Statement { statement = value });
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
			                    value = query.Value
			                };
						  
			foreach(var select in inserts)
			{
				string id = select.id.Trim();
				string value = select.value.Trim();

				AddStatement(id, new Statement { statement = value });
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
			                    value = query.Value
			                };
						  
			foreach(var select in updates)
			{
				string id = select.id.Trim();
				string value = select.value.Trim();

				AddStatement(id, new Statement { statement = value });
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
			                    value = query.Value
			                };
						  
			foreach(var select in deletes)
			{
				string id = select.id.Trim();
				string value = select.value.Trim();

				AddStatement(id, new Statement { statement = value });
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
							  value = query.Value
						  };

			foreach (var select in deletes)
			{
				string id = select.id.Trim();
				string value = select.value.Trim();

				AddStatement(id, new Statement { statement = value });
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
				LoadQueries(file);
			}
		}
		#endregion
	}
}
