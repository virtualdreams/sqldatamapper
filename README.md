SqlDataMapper
=============

Simple library to map sql result sets into C# classes. Can also map C# classes into parameter objects.

Usage
-----

	use SqlDataMapper;
	
	namespace Demo
	{
		class Data
		{
			public int Id { get; set; }
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
	<sqlMapConfig>
		<providers file="./providers.xml" />
		<database provider="mysql" connectionString="Server=hostname;Database=database;Uid=username;Pwd=password;Pooling=true" />
		<sqlMaps>
			<sqlMap file="./sql.xml" />
		</sqlMaps>
	</sqlMapConfig>

This is the configuration for zeroconf.

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
	<sqlMap>
		<select id="getSome">
			<![CDATA[
			<your select statemnent here>
			]]>
		</select>
		<insert id="insertSome">
			<![CDATA[
			<your insert statemnent here>
			]]>
		</insert>
		<update id="updateSome">
			<![CDATA[
			<your update statemnent here>
			]]>	
		</update>
		<delete id="deleteSome">
			<![CDATA[
			<your delete statemnent here>
			]]>	
		</delete>
		<part id="partSome">
			<![CDATA[
			<your part statemnent here>
			]]>	
		</part>
	</sqlMap>

