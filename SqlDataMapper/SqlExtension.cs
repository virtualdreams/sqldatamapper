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
		[Obsolete("Not officially supported")]
		public static SqlQuery SetParameter<TSource>(this SqlQuery query, TSource value) where TSource : class
		{
			if (query == null)
				throw new ArgumentNullException("query");

			if (value == null)
				throw new ArgumentNullException("value");

			var properties = typeof(TSource).GetProperties(BindingFlags.GetProperty | BindingFlags.Public | BindingFlags.Instance);
			foreach (var property in properties)
			{
				var ignore = property.GetCustomAttributes(typeof(IgnoreAttribute), true).Length > 0;
				if (!ignore && property.CanRead)
				{
					var column = new Column(property);

					query.SetParameter(column.Name, column.GetValue(value));
				}
			}

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

	/// <summary>
	/// Extension to convert a byte array to hexadecimal.
	/// 
	/// This function is much more faster than "BitConverter.ToString(byte[]).Replace("-", String.Empty)"
	/// http://stackoverflow.com/questions/623104/byte-to-hex-string/3974535#3974535
	/// </summary>
	internal static class HexExtension
	{
		/// <summary>
		/// Convert a byte array to hex.
		/// </summary>
		/// <param name="bytes">Array of bytes.</param>
		/// <returns>Hexadecimal string.</returns>
		public static string ToHex(this byte[] bytes)
		{
			char[] c = new char[bytes.Length * 2];

			byte b;

			for (int bx = 0, cx = 0; bx < bytes.Length; ++bx, ++cx)
			{
				b = ((byte)(bytes[bx] >> 4));
				c[cx] = (char)(b > 9 ? b + 0x37 + 0x20 : b + 0x30);

				b = ((byte)(bytes[bx] & 0x0F));
				c[++cx] = (char)(b > 9 ? b + 0x37 + 0x20 : b + 0x30);
			}

			return new string(c);
		}

		/// <summary>
		/// Convert a string to byte array.
		/// </summary>
		/// <param name="str">The hexadecimal string.</param>
		/// <returns>Byte array.</returns>
		public static byte[] HexToBytes(this string str)
		{
			if (str.Length == 0 || str.Length % 2 != 0)
				return new byte[0];

			byte[] buffer = new byte[str.Length / 2];
			char c;
			for (int bx = 0, sx = 0; bx < buffer.Length; ++bx, ++sx)
			{
				// Convert first half of byte
				c = str[sx];
				buffer[bx] = (byte)((c > '9' ? (c > 'Z' ? (c - 'a' + 10) : (c - 'A' + 10)) : (c - '0')) << 4);

				// Convert second half of byte
				c = str[++sx];
				buffer[bx] |= (byte)(c > '9' ? (c > 'Z' ? (c - 'a' + 10) : (c - 'A' + 10)) : (c - '0'));
			}

			return buffer;
		}
	}
}
