SqlDataMapper
=============

Simple library to map sql result sets into C# classes.

Usage
-----

Sample code to work with the library.

	use SqlDataMapper;
	
	namespace Demo
	{
		class Data
		{
			[Alias("Table_ID")]
			[NotNull]
			public int Id { get; set; }
			
			[Required]
			public string Name { get; set; }
		}
		
		class Sample
		{
			public void Demo()
			{
				var _config = new SqlConfig();
				var _context = _config.CreateContext();
				var _query = _config.CreateQuery("getSome");
				
				var _result = _context.QueryForList<Data>(_query);
			}
		}
	}


Config
------

Create a file named *SqlMapperConfig.xml* with the following content:

	<?xml version="1.0" encoding="utf-8" ?>
	<configuration>
		<provider file="./providers.xml" />
		<connection provider="mysql" connectionString="Server=hostname;Database=database;Uid=username;Pwd=password;Pooling=true" />
		<statements>
			<include file="./sql.xml" />
			<statement id="someOtherId">
				<![CDATA[
				select
					*
				from
					table
				]]>
			</statement>
		</statements>
	</configuration>

Create also a file named *providers.xml* with the following content:

	<?xml version="1.0" encoding="utf-8" ?>
	<providers>
		<provider 
			id="mysql" 
			assemblyName="MySql.Data" 
			connectionClass="MySql.Data.MySqlClient.MySqlConnection" 
		/>
		<provider
			id="mssql2.0"
			assemblyName="System.Data, Version=2.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089"
			connectionClass="System.Data.SqlClient.SqlConnection"
		/>
		<provider 
			id="oracle" 
			assemblyName="Devart.Data.Oracle" 
			connectionClass="Devart.Data.Oracle.OracleConnection" 
		/>
		<provider 
			id="sqlite3" 
			assemblyName="System.Data.SQLite" 
			connectionClass="System.Data.SQLite.SQLiteConnection" 
		/>
	</providers>
	
This file contains the assembly names and the class names to initiate the connection to the database.

The last file can named with every name, i.e.: *sql.xml*. Some sample content:

	<?xml version="1.0" encoding="UTF-8"?>
	<statements>
		<statement id="getAllItems">
			<![CDATA[
			select
				*
			from
				table
			]]>
		</statement>
		<statement id="getItem">
			<![CDATA[
			select
				*
			from
				table
			where
				id = @id
			]]>
		</statement>
		<statement id="updateItem">
			<![CDATA[
			update
				table
			set
				name = @name
			where
				id = @id
			]]>	
		</statement>
		<statement id="deleteItem">
			<![CDATA[
			delete from
				table
			where
				id = @id
			]]>	
		</statement>
		<statement id="partSome">
			<![CDATA[
			and name like @term
			and number like @term
			]]>	
		</statement>
	</statements>