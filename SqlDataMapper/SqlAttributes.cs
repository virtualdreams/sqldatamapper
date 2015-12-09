using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SqlDataMapper
{
	/// <summary>
	/// Ignore this property.
	/// </summary>
	[AttributeUsage(AttributeTargets.Property, AllowMultiple = false)]
	public class IgnoreAttribute : Attribute
	{ }

	/// <summary>
	/// The result column is required.
	/// </summary>
	[AttributeUsage(AttributeTargets.Property, AllowMultiple = false)]
	public class RequiredAttribute : Attribute
	{ }

	/// <summary>
	/// The result column must not be null.
	/// </summary>
	[AttributeUsage(AttributeTargets.Property, AllowMultiple = false)]
	public class NotNullAttribute : Attribute
	{ }

	/// <summary>
	/// Alias name for a property.
	/// </summary>
	[AttributeUsage(AttributeTargets.Property, AllowMultiple = false)]
	public class AliasAttribute : Attribute
	{
		/// <summary>
		/// The alias name for an column.
		/// </summary>
		public string Name { get; set; }

		/// <summary>
		/// Initialize an empty alias.
		/// </summary>
		public AliasAttribute()
		{
			Name = String.Empty;
		}

		/// <summary>
		/// Initialize alias with given name.
		/// </summary>
		/// <param name="name"></param>
		public AliasAttribute(string name)
		{
			if (String.IsNullOrEmpty(name))
				Name = String.Empty;
			else
				Name = name;
		}
	}
}
