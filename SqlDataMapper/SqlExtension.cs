using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Xml;
using System.Xml.Linq;
using System.Xml.Schema;

namespace SqlDataMapper.Extension
{
	/// <summary>
	/// Extension for the <c>SqlXmlLoader</c>.
	/// </summary>
	public static class SqlXmlLoaderExtension
	{
		/// <summary>
		/// Load configuration from file.
		/// </summary>
		/// <param name="config">The config object.</param>
		/// <param name="filename">The filename.</param>
		public static void Configure(this SqlConfig config, string filename)
		{
			new SqlXmlLoader(config).Configure(filename);
		}

		/// <summary>
		/// Load providers from file.
		/// </summary>
		/// <param name="config">The config object.</param>
		/// <param name="filename">The filename.</param>
		public static void AddProviders(this SqlConfig config, string filename)
		{
			new SqlXmlLoader(config).AddProviders(filename);
		}

		/// <summary>
		/// Load statements from file.
		/// </summary>
		/// <param name="config">The config object.</param>
		/// <param name="filename">The filename.</param>
		public static void AddStatements(this SqlConfig config, string filename)
		{
			new SqlXmlLoader(config).AddStatements(filename);
		}
	}

	/// <summary>
	/// Set properties from an class as parameters.
	/// </summary>
	public static class SqlQueryExtension
	{
		/// <summary>
		/// Set properties from an class as parameters.
		/// </summary>
		/// <typeparam name="TSource">The source.</typeparam>
		/// <param name="query">The query instance.</param>
		/// <param name="value">The value.</param>
		/// <returns>This instance.</returns>
		public static SqlQuery SetParameter<TSource>(this SqlQuery query, TSource value) where TSource : class
		{
			if (query == null)
				throw new ArgumentNullException("query");

			if (value == null)
				throw new ArgumentNullException("value");

			var state = query.SuppressException;
			query.SuppressException = true;
			var properties = typeof(TSource).GetProperties(BindingFlags.GetProperty | BindingFlags.Public | BindingFlags.Instance);
			foreach (var property in properties)
			{
				var column = new Column(property);

				query.SetParameter(column.Name, column.GetValue(value));
			}

			query.SuppressException = state;
			return query;
		}
	}

	/// <summary>
	/// Extension for <c>XDocument</c>.
	/// </summary>
	internal static class XDocumentExtension
	{
		/// <summary>
		/// Validate document against schema (embedded resource).
		/// </summary>
		/// <param name="document">The xml document.</param>
		/// <param name="schema">The schema resource.</param>
		public static void Validate(this XDocument document, string schema)
		{
			if (document == null)
				throw new ArgumentNullException("doc");

			if (String.IsNullOrEmpty(schema))
				throw new ArgumentNullException("schema");

			XmlSchemaSet schemas = new XmlSchemaSet();
			Stream xsdStream = Assembly.GetExecutingAssembly().GetManifestResourceStream(schema);
			if (xsdStream != null)
			{
				schemas.Add(null, XmlReader.Create(xsdStream));
			}

			document.Validate(schemas, (o, e) =>
			{
				throw new SqlDataMapperException(String.Format("Validate xml failed: -> {0}.", e.Message), e.Exception);
			});
		}
	}

	/// <summary>
	/// Extension for <c>string</c> to convert a filename to full path.
	/// </summary>
	internal static class StringExtension
	{
		/// <summary>
		/// Get the full path for the given filename.
		/// </summary>
		/// <param name="filename">The filename as file, absolute filepath or relative filepath</param>
		/// <returns>The full path</returns>
		public static string ConvertToFullPath(this string filename)
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
	}
}
