using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Linq;
using SqlDataMapper.Extension;

namespace SqlDataMapper
{
	/// <summary>
	/// Extension class to configure <c>SqlConfig</c> from xml.
	/// </summary>
	public class SqlXmlLoader
	{
		private SqlConfig Config { get; set; }

		/// <summary>
		/// Initialize a new instance of the SqlXmlLoader.
		/// </summary>
		/// <param name="config">The config instance.</param>
		public SqlXmlLoader(SqlConfig config)
		{
			if (config == null)
				throw new ArgumentNullException("config");

			Config = config;
		}

		/// <summary>
		/// Load configuration from file.
		/// </summary>
		/// <param name="filename">The filename.</param>
		public void Configure(string filename)
		{
			if (String.IsNullOrEmpty(filename))
				throw new ArgumentNullException("filename");

			filename = filename.ConvertToFullPath();

			XDocument configuration = XDocument.Load(filename, LoadOptions.None);

			configuration.Validate("SqlDataMapper.Schemas.configuration.xsd");

			ReadConfiguration(configuration);
		}

		/// <summary>
		/// Read configuration from xml.
		/// </summary>
		/// <param name="document"></param>
		private void ReadConfiguration(XDocument document)
		{
			string providerFilename = document.Element("configuration").Element("provider").Attribute("file").Value;
			var database = document.Element("configuration").Element("connection");
			string providerName = database.Attribute("provider").Value;
			string connectionString = database.Attribute("connectionString").Value;
			var statementsNode = document.Element("configuration").Element("statements");

			// read providers
			LoadProvider(providerFilename);

			// read statements and includes if node available
			if (statementsNode != null)
			{
				ReadStatements(statementsNode);
			}

			// assign default provider and connection string
			Config.DefaultProvider = providerName;
			Config.DefaultConnectionString = connectionString;
		}

		/// <summary>
		/// Load providers from file.
		/// </summary>
		/// <param name="filename">The filename.</param>
		public void LoadProvider(string filename)
		{
			if (String.IsNullOrEmpty(filename))
				throw new ArgumentNullException("filename");

			filename = filename.ConvertToFullPath();

			XDocument provider = XDocument.Load(filename);

			provider.Validate("SqlDataMapper.Schemas.provider.xsd");

			ReadProvider(provider);
		}

		/// <summary>
		/// Read providers from xml.
		/// </summary>
		/// <param name="document"></param>
		private void ReadProvider(XDocument document)
		{
			var providers = from provider in document.Root.Elements("provider")
							select new
							{
								Id = provider.Attribute("id").Value.ToString(),
								AssemblyName = provider.Attribute("assemblyName").Value.ToString(),
								ConnectionClass = provider.Attribute("connectionClass").Value.ToString()
							};

			foreach (var provider in providers)
			{
				Config.AddProvider(provider.Id.Trim(), provider.AssemblyName.Trim(), provider.ConnectionClass.Trim());
			}
		}

		/// <summary>
		/// Load statements from file.
		/// </summary>
		/// <param name="filename">The filename.</param>
		public void LoadStatements(string filename)
		{
			if (String.IsNullOrEmpty(filename))
				throw new ArgumentNullException("filename");

			filename = filename.ConvertToFullPath();

			XDocument statements = XDocument.Load(filename);

			statements.Validate("SqlDataMapper.Schemas.statements.xsd");

			ReadStatements(statements.Root);
		}

		/// <summary>
		/// Read statements and includes from xml.
		/// </summary>
		/// <param name="root"></param>
		private void ReadStatements(XElement root)
		{
			var statements = from node in root.Elements("statement")
							 select new
							 {
								 Id = node.Attribute("id").Value.ToString(),
								 Content = node.Value
							 };

			var includes = from node in root.Elements("include")
						   select new
						   {
							   File = node.Attribute("file").Value.ToString()
						   };

			foreach (var statement in statements)
			{
				Config.AddStatement(statement.Id.Trim(), statement.Content.Trim());
			}

			foreach (var include in includes)
			{
				LoadStatements(include.File.Trim());
			}
		}
	}
}
